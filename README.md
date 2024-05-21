# ContinuousMonitory

### Description

An introduction to Continuous Monitory with logging, metrics and tracing using OpenTelemetry, Prometheus, and Grafana

* OpenTelemetry is an open source standard for capturing metrics about an appLication.
* Prometheus is an open source software that records metrics in a time series database built using an HTTP pull model, with flexible queries and real-time alerting.
* Grafana is an open source visualization tool which can be used to view the captured metrics.
* Loki is an open source log aggregation system designed to store and query logs from all your applications and infrastructure.
    * It works with Promtail as platform that sends logs to Loki. Loki is the logging engine.
* OpenTelemetry Collector is a vendor-agnostic implementation of how to receive, process and export telemetry data. 
It removes the need to run, operate, and maintain multiple agents/collectors. This works with improved scalability and 
supports open source observability data formats (e.g. Jaeger, Prometheus, Fluent Bit, etc.) sending to one or more open source or commercial backends.

* Install Grafana locally
    * Go to https://grafana.com/grafana/download and download Grafana Enterprise version. 
    It is free to download but you pay for enterprise features if you need them.
    * Run the installation. You can just click next to completion.
    * Navigate to http://locahost:3000 (default url for Grafana).
        * If it is not available, go to the Services window and start the Grafana Service.

* Install Prometheus
    * Go to https://prometheus.io/download/ and download latest Prometheus version.
    * Extract the installation.
    * Using the Prometheus configuration yaml file, copy it to the same directory as Prometheus
    * Run "./prometheus --config.file=prometheus.yml" to start up Prometheus.
    * Navigate to http://localhost:9090 (default url for Prometheus).

* Install Loki and Promtail
    * Go to https://grafana.com/docs/loki/latest/setup/install/local/ and instal Loki manually by downloading the lastest version off Github.
        * Download links: 
            * https://github.com/grafana/loki/releases/download/v2.9.8/loki-windows-amd64.exe.zip
            * https://github.com/grafana/loki/releases/download/v2.9.8/promtail-windows-amd64.exe.zip
    * Extract both zips into a singe directory. The name does not matter.
    * Copy and paste the commands below into your command line to download generic configuration files. Use the corresponding Git refs that match 
    your downloaded Loki version to get the correct configuration file. For example, if you are using Loki version 2.9.2, you need to use the 
    https://raw.githubusercontent.com/grafana/loki/v2.9.2/cmd/loki/loki-local-config.yaml URL to download the configuration file that corresponds 
    to the Loki version you aim to run.
    * Run ".\loki-windows-amd64.exe --config.file=loki-local-config.yaml" to start Loki
    * Run ".\promtail-windows-amd64.exe --config.file=promtail-local-config.yaml" to start Promtail
    * You can also follow the instructions on https://grafana.com/docs/loki/latest/setup/install/local/.

* Install OpenTelemetry Collector
    * Run the below docker command inside OpenTelemetoryConnector folder in this repository.
        * docker run -p 4317:4317 -p 4318:4318 --rm -v "C:/Users/Jarryd Deane/source/repos/ContinuousMonitory/OpenTelemetryCollector/collector-config.yaml:/etc/otelcol/config.yaml" otel/opentelemetry-collector

### ContinuousMonitory

* Start project and it should bring up swagger on https://localhost:7298.
* Navigate to Prometheus http://localhost:9090 and go to Status -> Targets.
* Click on the application link with https://localhost:7298/ as it's url to view metrics.
* After installing and running OpentTelemetry Collector and the ContinuousMonitory Api, logs, metrics and traces should be logged to the console
as the received. 

### Setup Prometheus as data source on Grafana and import Dashboards

* Go to Grafana url and Home -> Connections -> Add new connections.
* Specify the Prometheus url http://localhost:9090 and save and test connection.
* Go to https://grafana.com/orgs/dotnetteam and click view details for one of the 2 dashboards
and copy the Dashboard ID or download the json. 
* Go back to Grafana and import the dashboard using the Dashboard ID or json.
* You can import the dashboard by going to Dashboards -> New -> Import and specifying the code 19924 or 19925.

### Setup Loki as data source on Grafana

* Go to Grafana url and Home -> Connections -> Add new connections.
* Specify the Prometheus url http://localhost:3100 and save and test connection.
* Go to Explore and select Loki as datasource to start querying logs.

### References
* https://grafana.com/docs/grafana/latest/setup-grafana/installation/windows/
* https://prometheus.io/docs/prometheus/latest/getting_started/
* https://prometheus.io/download/
* https://grafana.com/orgs/dotnetteam
* https://grafana.com/docs/loki/latest/setup/install/local/
* https://opentelemetry.io/docs/collector/