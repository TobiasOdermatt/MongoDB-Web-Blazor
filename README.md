# Online MongoDB Dashboard
OnlineMongoDBManagement is a web-based application that allows users to manage their MongoDB databases online. 
<br>It will be similar to phpMyAdmin, but instead of dealing with MySQL databases, it works with MongoDB.
<br>Data is saved using an One Time Pad (OTP) which encrypts the data.

## Features
- CRUD operations for databases, collections, and documents
- Data explorer
- Upload / Download JSON
- Index manager
- Collection statistics
- Database statistics
- Server statistics
- Built-in login (OTP) (optional)
- Activity logs (optional)

 All the preferences which services you wanna include is in the config.properties

 Certainly, here's the content you can put in your GitHub documentation in Markdown format:

## Installation Guide for Docker

## Build and Run Docker Container
Run the following commands:

```
docker build -t mongodb-web-client .
docker run -p 8080:80 -d mongodb-web-client
```

## Access MongoDB from Host on Mac/Windows
If you wish to connect to your MongoDB server from your host machine, update the `config.properties` file's database host to:

```
host.docker.internal
```
