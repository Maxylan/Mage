#!/bin/bash

root_path=$(pwd)
# Ensure that this script has been run from project root (because we do 'cd' commands).
if [ ! -f "$root_path"/setup.sh ] || [ ! -f "$root_path"/specialized-setup.sh ]; then
    echo -e "\nThe script needs to be run from the project's root. You ran it from \"$root_path\". Exiting..\n"
    exit 2
fi

if [ ! -f "$root_path"/.env ]; then
    if [ ! -f "$root_path"/sample.env ]; then
        echo -e "\nMissing 'sample.env'."
        echo -e "Ensure the script was run from the project's root. You ran it from \"$root_path\". Exiting..\n"
        exit 3
    fi

    cp -pv "$root_path"/sample.env "$root_path"/.env
fi

source .env

if [ ! -f "$STORAGE_MOUNT_POINT"/.env ]; then
    if [ ! -f "$STORAGE_MOUNT_POINT"/sample.env ]; then
        echo -e "\nMissing '$STORAGE_MOUNT_POINT/sample.env'."
        echo -e "Ensure the script was run from the project's root. You ran it from \"$root_path\". Exiting..\n"
        exit 3
    fi

    cp -pv "$STORAGE_MOUNT_POINT"/sample.env "$STORAGE_MOUNT_POINT"/.env
fi

if [ ! -f "$RECEPTION_MOUNT_POINT"/appsettings.json ]; then
    if [ ! -f "$RECEPTION_MOUNT_POINT"/sample.appsettings.json ]; then
        echo -e "\nMissing '$RECEPTION_MOUNT_POINT/sample.appsettings.json'."
        echo -e "Ensure the script was run from the project's root. You ran it from \"$root_path\". Exiting..\n"
        exit 3
    fi

    cp -pv "$RECEPTION_MOUNT_POINT"/sample.appsettings.json "$RECEPTION_MOUNT_POINT"/appsettings.json
fi

if [ ! -f "$RECEPTION_MOUNT_POINT"/appsettings.Development.json ]; then
    if [ ! -f "$RECEPTION_MOUNT_POINT"/sample.appsettings.Development.json ]; then
        echo -e "\nMissing '$RECEPTION_MOUNT_POINT/sample.appsettings.Development.json'."
        echo -e "Ensure the script was run from the project's root. You ran it from \"$root_path\". Exiting..\n"
        exit 3
    fi

    cp -pv "$RECEPTION_MOUNT_POINT"/sample.appsettings.Development.json "$RECEPTION_MOUNT_POINT"/appsettings.Development.json
fi

if [ ! -d "$GATE_MOUNT_POINT"/dist ]; then
    cd "$GATE_MOUNT_POINT" || return;
    echo -e "\nInstalling \"$GATE_NAME\"..\n"
    npm i
    cd "$root_path" || return;
fi

if [ ! -d "$GARDEN_MOUNT_POINT"/dist ]; then
    cd "$GARDEN_MOUNT_POINT" || return;
    echo -e "\nInstalling \"$GARDEN_NAME\"..\n"
    npm i
    cd "$root_path" || return;
fi

echo -e "\nDone!\n"