# aria2mqtt

**aria2mqtt** is a bridge between Fitbit Aria Wi-Fi scales and MQTT, featuring Home Assistant discovery. It intercepts weight data from traffic intended for Fitbit servers and relays the request to ensure normal scale functionality. The extracted data is then sent via MQTT, where it can be automatically picked up by Home Assistant.

## Requirements

To use **aria2mqtt**, you need the ability to define custom DNS rules on your network to redirect `www.fitbit.com` to a Docker container on your local network.

For example, I use Pi-hole to achieve this. My network's DHCP is configured to use Pi-hole as its DNS server. In Pi-hole, I create a record in the *Local DNS* -> *DNS Records* section that points `www.fitbit.com` to a Traefik reverse proxy running in a Docker container. This setup allows the request from the scales on port 80 to be redirected to a custom IP and port where the aria2mqtt container is running.

## Installation

A Dockerfile and Docker Compose file are provided. Ensure that you have configured the correct environment variables in your `.env` file.

## Environment Variables

To run this project, you need to add the following environment variables to your `.env` file:

- `FITBIT_URL` this will need to be an ip address, you can get this from pinging `www.fitbit.com` before setting up your DNS rules, at time of writing this is `http://35.244.211.136`
- `MQTT_SERVER`
- `MQTT_USER`
- `MQTT_PASSWORD`
