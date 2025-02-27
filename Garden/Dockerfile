FROM httpd:2.4-alpine

RUN apk --no-cache add bash 

RUN mkdir -p /var/log/apache2/mage
RUN chown www-data:www-data -R /var/log/apache2/mage

# COPY ./mage.conf /usr/local/apache2/conf/httpd.conf
# COPY ./dist/guard/ /usr/local/apache2/htdocs/
