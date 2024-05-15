# ContinuousMonitory

### Description

An introduction to Continuous Monitory with logging, metrics and tracing using OpenTelemetry, Prometheus, and Grafana

* OpenTelemetry is an open source standard for capturing metrics about an appLication.
* Prometheus is an open source software that records metrics in a time series database built using an HTTP pull model, with flexible queries and real-time alerting.
* Grafana is an open source visualization tool which can be used to view the captured metrics.

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

### References
* https://grafana.com/docs/grafana/latest/setup-grafana/installation/windows/
* https://prometheus.io/docs/prometheus/latest/getting_started/