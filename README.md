# 📊 Metric Exporter

Microservicio en .NET 8.0 para exponer métricas personalizadas desde InfluxDB en formato Prometheus...

## 🚀 Arquitectura

Este microservicio es parte de una arquitectura **event-driven basada en microservicios**. Cumple la función de **exponer métricas agregadas** de eventos almacenados en InfluxDB para ser consumidas por **Prometheus + Grafana**.

---

# Componente	Descripción
Lenguaje	        C# (.NET 8.0)
Framework	        ASP.NET Core Web API
Base de Datos	    InfluxDB (lectura de métricas del bucket eventos)
Puerto de escucha	http://0.0.0.0:5000
Ruta Prometheus	    GET /metrics – devuelve métricas con formato Prometheus
Dependencias	    InfluxDB.Client, Swashbuckle.AspNetCore, Microsoft.Extensions.Configuration.Json
