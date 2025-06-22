#!/usr/bin/env node

import fs from 'node:fs';

/**
 * Turns a `{string}` into a snake-case format (optionally with a custom separator).
 *
 * @param   {string} str
 * @param   {string} sep
 * @returns {string}
 */
function toSnakeCase(str, sep = '-') {
    if (typeof str !== 'string') {
        return '';
    }

    let snakeCase = '';
    for (let i = 0; i < str.length; i++) {
        const char = str.charAt(i);

        if (char.toLowerCase() != char.toUpperCase() &&
            [" ", "_", "~", "|", "\n"].includes(char)
        ) {
            snakeCase += sep;
        }
        else if (char === char.toUpperCase()) {
            snakeCase += sep + char.toLowerCase();
        }
        else {
            snakeCase += char;
        }
    }

    let i = 0;
    while (snakeCase.indexOf(sep) === 0 && ++i < str.length) {
        snakeCase = snakeCase.substring(1);
    }

    return snakeCase;
}

/**
 * Turns a `{string}` into a filename snake-case format.
 *
 * @param   {string} str
 * @returns {string}
 */
function toFileName(str) {
    let fileName = toSnakeCase(str, '-');
    return fileName.replace('d-t-o', 'dto');
}

/**
 * Turns a `{string}` into CamelCase format.
 *
 * @param   {string} str
 * @returns {string}
 */
function toCamelCase(str) {
    if (typeof str !== 'string') {
        return '';
    }

    let camelCase = '';
    for (let i = 0; i < str.length; i++) {
        let char = str.charAt(i);

        if (char.toLowerCase() != char.toUpperCase() &&
            [" ", "-", "_", "~", "|", "\n"].includes(char)
        ) {
            let char = str.charAt(++i);

            if (char === char.toLowerCase()) {
                camelCase += char.toUpperCase();
            }
        }
        else {
            camelCase += char;
        }
    }

    return camelCase;
}

/**
 * Converts a single OpenAPI 3.1 Schema Property into a typescript enum.
 *
 * @returns {[string|null, string|null]}
 */
function formatRefType(ref) {
    const formattedRef = [null, null];

    if (ref && typeof ref === 'string') {
        ref = ref
            .normalize()
            .trim();
                    
        formattedRef[0] =
            toCamelCase(ref.replace('#/components/schemas/', ''));

        if (formattedRef[0].endsWith('DTO')) {
            formattedRef[1] = `import { I${formattedRef[0]} } from './${toFileName(formattedRef[0])}';`;
            formattedRef[0] = 'I' + formattedRef[0];
        }
        else {
            formattedRef[1] = `import { ${formattedRef[0]} } from './${toFileName(formattedRef[0])}';`;
        }
    }

    return formattedRef;
}

/**
 * Converts a single OpenAPI 3.1 Schema Property into a typescript enum.
 *
 * @returns {string[]}
 */
function singleEnum(name, enumProperty, indent = 0) {
    return [
        ' '.repeat(indent) + `export enum ${name} {`,
        ...enumProperty.map(value => ' '.repeat(indent + 4) + `${value} = '${value}',`),
        ' '.repeat(indent) + '}'
    ];
}

/**
 * Converts a single OpenAPI 3.1 Schema Property into a typescript type.
 *
 * @returns {[string, string[]]}
 */
function singleType(propertyEntry, isDataTransferObject, indent = 0) {
    const [
        name,
        definition
    ] = propertyEntry;

    let {
        type,
        format,
        maxLength,
        minLength,
        nullable,
        readOnly,
        items,
        $ref
    } = definition;

    const refs = [];
    const content = [' '.repeat(indent - 1)];

    if (maxLength !== undefined || minLength !== undefined) {
        content.push("/**\n" + (' '.repeat(indent - 1)));

        if (maxLength !== undefined) {
            content.push(" * Max Length: " + maxLength + "\n" + (' '.repeat(indent - 1)));
        }
        if (minLength !== undefined) {
            content.push(" * Min Length: " + minLength + "\n" + (' '.repeat(indent - 1)));
        }

        content.push(" */\n" + (' '.repeat(indent - 1)));
    }
 
    if (readOnly) {
        content.push('readonly');
    }

    if (isDataTransferObject && nullable) {
        content.push(name + '?:');
    }
    else {
        content.push(name + ':');
    }


    if (type) {
        switch(type) {
            case 'float':
            case 'double':
            case 'decimal':
            case 'integer':
                type = 'number'
                break;
            case 'string':
                if (format) {
                    switch(format) {
                        case 'date-time':
                            type = 'Date'
                            break;
                    }
                }
                break;
            case 'array':
                // Might be recursive, but I don't care.
                if ('type' in items) {
                    switch(items.type) {
                        case 'float':
                        case 'double':
                        case 'decimal':
                        case 'integer':
                            type = 'number'
                            break;
                        case 'string':
                            type = 'string[]';
                            if (format) {
                                switch(format) {
                                    case 'date-time':
                                        type = 'Date[]'
                                        break;
                                }
                            }
                            break;
                        default:
                            type = items.type + '[]';
                            break;
                    }
                }
                else if ('$ref' in items) {
                    const [
                        refTypeName,
                        refImport
                    ] = formatRefType(items.$ref);

                    refs.push(refImport);
                    type = refTypeName + '[]';
                }
                break;
        }
    }
    else if ($ref && typeof $ref === 'string') {
        const [
            refTypeName,
            refImport
        ] = formatRefType($ref);

        refs.push(refImport);
        type = refTypeName;
    }

    content.push(type);

    if (nullable) {
        content.push('| null');
    }

    return [
        content.join(' '),
        refs
    ];
}

/**
 * Generates a new "type definition" (typescript file) out of a single Open API 3.1 Schema
 *
 * @returns {[string, string[]]}
 */
function generateTypeDefinition(schemaEntry, hasDTO) {
    let [
        schemaName,
        schemaProperties
    ] = schemaEntry;

    let {
        type,
        properties,
        additionalProperties,
    } = schemaProperties;

    schemaName = schemaName
        .normalize()
        .trim();

    const fileName = toFileName(schemaName);
    const typeName = toCamelCase(schemaName);
    const isDTO = !hasDTO && typeName.endsWith('DTO');
    let content = [];

    if (properties) {
        if (hasDTO) {
            content.push(`import { I${typeName}DTO } from './${toFileName(typeName + 'DTO')}';`);
            content.push(`export type ${typeName} = I${typeName}DTO & {`);
        }
        else if (isDTO) {
            content.push(`export interface I${typeName} {`);
        }
        else {
            content.push(`export type ${typeName} = {`);
        }

        for (const propertyEntry of Object.entries(properties)) {
            const [
                type,
                refs
            ] = singleType(propertyEntry, isDTO, 4)

            for (const ref of refs) {
                if (!content.includes(ref)) {
                    content = [ref, ...content];
                }
            }

            content.push(type + ',');
        }

        content.push('}');
    }

    if ('enum' in schemaProperties) {
        if (content.length > 0) {
            content.push('');
        }

        content = content.concat(
            singleEnum(typeName, schemaProperties['enum'])
        );
    }

    const exportLine = content.findIndex(
        line => (
            line.startsWith('export type') ||
            line.startsWith('export interface') ||
            line.startsWith('export enum')
        )
    );

    if (exportLine > 0) {
        content[exportLine] = "\n" + content[exportLine];
    }
    
    return [fileName + '.ts', content];
}

/**
 * Writes a new "type definition" (typescript file) to disk.
 *
 * @returns {boolean}
 */
function write(definition) {
    const [
        name,
        content
    ] = definition;


    if (!name) {
        console.error('Skipping file with no name!');
        return false;
    }
    if (fs.existsSync(name)) {
        console.warn(`Skipping file '${name}' because it existed.`);
        return false;
    }

    if (!name.endsWith('.ts')) {
        name += '.ts';
    }

    try {
        fs.writeFileSync(name, content.join("\n"));
    }
    catch(err) {
        return false;
    }

    return true;
}

(async (api_url, dir) => {
    const openApiDefinition = await fetch(api_url, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(res => {
            console.log('Acquired OpenAPI 3.1 Definition..');
            return res.json();
        })
        .catch(err => {
            console.error('Failed to acquire OpenAPI 3.1 Definition..', err);
            return err;
        });

    if (
        !('components' in openApiDefinition) ||
        !('schemas' in openApiDefinition.components)
    ) {
        console.error('Invalid OpenAPI 3.1 Definition..', openApiDefinition);
        return;
    }

    const schemaEntries = Object.entries(
        openApiDefinition.components.schemas
    );

    const logger = (entries => {
        let latest = {
            progress: null,
            invoke: () => console.log(`#${entries.length} Schema Entries..`)
        };
        const logs = [];

        return function(consoleLog, progress) {
            console.clear();

            if (consoleLog) {
                logs.push(latest);
                latest = {
                    invoke: consoleLog
                };
            }

            for(const log of logs) {
                if (log) {
                    log.invoke(log.progress);
                }
            }

            latest.progress = progress || null;
            latest.invoke(latest.progress);
        }
    })(schemaEntries)

    logger(progress => console.log(`#${progress||0} Type Definitions..`));
    const typeDefinitions = await Promise.all(
        schemaEntries.map(
            async (entry, i) => {
                const hasDTO = (
                    !entry[0].endsWith('DTO') &&
                    schemaEntries.some(_ => _[0] === (entry[0] + 'DTO'))
                );

                const definition = generateTypeDefinition(entry, hasDTO);
                logger(null, i);

                return definition;
            }
        )
    );

    if (!Array.isArray(typeDefinitions) || typeDefinitions.length <= 0) {
        logger(() => console.warn('Generated no type definitions.'));
        return;
    }

    if (!fs.existsSync(dir)) {
        try {
            fs.mkdirSync(dir);
            logger(() => console.log(`Created '${dir}' dir..`));
        }
        catch(err) {
            logger(() => console.log(`Failed to create dir '${dir}'`, err));
            throw err;
        }
    }

    // Writes definitions to disk as ts-files.
    const fileWrites = await Promise.all(
        typeDefinitions.map(async definition => {
            const fileName = dir + '/' + definition[0];
            let status = 'Working..';

            logger(() => console.log(`[${status}] File '${fileName}'`));

            const result = write(
                [ fileName, definition[1] ]
            );

            status = result
                ? 'Done!'
                : 'Failed!';

            logger(null, null);
            return result;
        })
    );
    
    logger(() => console.log(`Success: ${fileWrites.filter(result => !!result).length}/${fileWrites.length}`));

    if (fileWrites.some(result => !result)) {
        logger(() => console.log(`Fails: ${fileWrites.filter(result => !result).length}/${fileWrites.length}`));
    }
})('http://localhost/reception/swagger/v1/swagger.json', './_generated');
