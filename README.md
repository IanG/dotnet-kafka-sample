# DotNetKafkaSample

## Introduction
This is an example of how to produce and consume messages against an [Apache Kafka](https://kafka.apache.org/) topic using [.NET Worker Services](https://learn.microsoft.com/en-us/dotnet/core/extensions/workers).

If you find this repository useful please give it a ⭐️

The example is based around the simple concept of smart doors with sensors that exist within a building.  These doors produce events when they are opened and closed which are published to a Kafka topic.  These events are then consumed by interested parties.

The solution contains 3 projects:

- **Common** - this project contains POCO objects that represent events that can occur for smart doors.
- **Producer** - this project is a .NET worker service which randomly creates `DoorOpen` and `DoorClosed` events and produces them onto the configured Kafka topic
- **Consumer** - this project is a .NET worker service which consumes smart door events from the configured Kafka topic

**Note** at present this example does not cover more advanced concepts such as:
- Client Authentication
- Use of the Kafka Schema Registry for constraining message structures.
- Non-JSON message formats (ProtoBuf, Avro etc.)

## Setting Up Kafka In Docker

The easiest way to use this solution is with a single-node Kafka cluster running within [Docker](https://www.docker.com/).

### Pulling And Starting The Kafka Image

You can pull and run the Apache Kafka image with:

```
docker run --name kafka-node -p 9092:9092 apache/kafka:latest
```

The Kafka node has a series of shell scripts within its `/opt/kafka/bin` directory which can be used for interacting with the cluster.  Connect to your running Kafka node with:

```
docker exec -it kafka-node /bin/sh
```

There are various Desktop and Web-based admin clients you can use for kafka instead if you wish but here are some simple commands to get you started.

#### Listing Existing Topics

```
/opt/kafka/bin/kafka-topics.sh --list --bootstrap-server "localhost:9092"
```

#### Creating A New Topic

```
/opt/kafka/bin/kafka-topics.sh --create --topic "door-events" --bootstrap-server "localhost:9092"
```

#### Delete An Existing Topic (And its Data !)

```
/opt/kafka/bin/kafka-topics.sh --delete --topic "door-events" --bootstrap-server "localhost:9092"
```

All of the scripts in the `/opt/kafka/bin` directory will provide you with help if you run them without any arguments.

## Message Production

The `Producer` project within the solution is a .NET Worker that will randomly produce door events.  

### Configuration
The `appsettings.json` file in this project contains the following configuration section which describes its configuration parameters for:

- **bootstrapservers** - the `<host:port>` list of Kafka brokers to connect to
- **clientid** - the Kafka identifier assigned to the client connection to the broker
- **topic** - the Kafka topic onto which new messages will be produced
- **pauseaftersendms** - how many milliseconds delay should occur between each message being sent

The JSON in `appsettings.json` should be as follows:

```
"DoorEventProducerWorker": {
  "bootstrapServers": "localhost:9092",
  "clientId": "DoorEventProducerWorker",
  "topic": "door-events",
  "PauseAfterSendMs": 1000
},
```
### Running The Producer

Open up a new terminal, move the todo the `Producer` directory and run:

```
dotnet run
```
You should now see output similar to this in the console:

```
[INF] [DotNetKafkaSample.Producer.Workers.DoorEventProducer] Producing DoorEvent messages to "localhost:9092" topic "door-events" as client "DoorEventProducerWorker"
[DBG] [DotNetKafkaSample.Producer.Workers.DoorEventProducer] Sending "DoorOpenEvent" event for door "1" with id 223eea0a-e399-46ca-87f1-3eb5aa8c7de8 to Kafka...
[INF] [Microsoft.Hosting.Lifetime] Application started. Press Ctrl+C to shut down.
[INF] [Microsoft.Hosting.Lifetime] Hosting environment: "Development"
[INF] [Microsoft.Hosting.Lifetime] Content root path: "./DotNetKafkaSample/Producer"
[DBG] [DotNetKafkaSample.Producer.Workers.DoorEventProducer] Event 223eea0a-e399-46ca-87f1-3eb5aa8c7de8 stored in partition 0 Offset 1
[DBG] [DotNetKafkaSample.Producer.Workers.DoorEventProducer] Pausing for 1000ms
[DBG] [DotNetKafkaSample.Producer.Workers.DoorEventProducer] Sending "DoorOpenEvent" event for door "8" with id 7409f762-9740-4c3e-9796-9f47a188d960 to Kafka...
[DBG] [DotNetKafkaSample.Producer.Workers.DoorEventProducer] Event 7409f762-9740-4c3e-9796-9f47a188d960 stored in partition 0 Offset 2
[DBG] [DotNetKafkaSample.Producer.Workers.DoorEventProducer] Pausing for 1000ms
[DBG] [DotNetKafkaSample.Producer.Workers.DoorEventProducer] Sending "DoorOpenEvent" event for door "2" with id a7cda6cb-6289-4a76-8ba6-561811f3a93a to Kafka...
[DBG] [DotNetKafkaSample.Producer.Workers.DoorEventProducer] Event a7cda6cb-6289-4a76-8ba6-561811f3a93a stored in partition 0 Offset 3
[DBG] [DotNetKafkaSample.Producer.Workers.DoorEventProducer] Pausing for 1000ms
```
You can stop the worker and new door event messages being produced with `ctrl+c`.

## Message Consumption

The `Consumer` project within the solution is a .NET Worker that will consume door events from the topic the `Producer` published events to.

### Configuration
The `appsettings.json` file in this project contains the following configuration section which describes its configuration parameters for:

- **bootstrap servers** - the `<host:port>` list of Kafka brokers to connect to
- **clientid** - the Kafka identifier assigned to the client connection to the broker
- **topic** - the Kafka topic onto which new messages will be produced
- **groupid** - the Kafka group id that this consumer belongs to

The JSON in `appsettings.json` should be as follows:

```
"DoorEventConsumerWorker": {
  "bootstrapServers": "localhost:9092",
  "clientId": "DoorEventConsumerWorker",
  "groupId": "DoorEventConsumerWorkerGroupId",
  "topic": "door-events"
},
```
### Running The Consumer

Open up a new terminal, move the todo the `Consumer` directory and run:

```
dotnet run
```
You should now see output similar to this in the console:

```
[INF] [DotNetKafkaSample.Consumer.Workers.DoorEventConsumer] Consuming DoorEvent messages from "localhost:9092" topic "door-events" as client "DoorEventConsumerWorker" for group "DoorEventConsumerWorkerGroupId"
[DBG] [DotNetKafkaSample.Consumer.Workers.DoorEventConsumer] Waiting for next message in topic "door-events"
[DBG] [DotNetKafkaSample.Consumer.Workers.DoorEventConsumer] Consuming "4be1ae8a-e34a-45fb-9d87-65415d0e7253" from partition 0 Offset 68674 Message "{\"event-type\":\"DoorClosed\",\"event-id\":\"4be1ae8a-e34a-45fb-9d87-65415d0e7253\",\"event-date-time\":\"2024-08-26T11:02:29.730262Z\",\"door-id\":\"1\"}"
[DBG] [DotNetKafkaSample.Consumer.Workers.DoorEventConsumer] Consuming event id 4be1ae8a-e34a-45fb-9d87-65415d0e7253 type "DoorClosedEvent" for door "1"
[DBG] [DotNetKafkaSample.Consumer.Workers.DoorEventConsumer] Waiting for next message in topic "door-events"
[DBG] [DotNetKafkaSample.Consumer.Workers.DoorEventConsumer] Consuming "5f1d9aa6-1bdb-494b-95a5-bdd2e3d1673b" from partition 0 Offset 68675 Message "{\"event-type\":\"DoorOpen\",\"event-id\":\"5f1d9aa6-1bdb-494b-95a5-bdd2e3d1673b\",\"event-date-time\":\"2024-08-26T11:02:29.738045Z\",\"door-id\":\"3\"}"
[DBG] [DotNetKafkaSample.Consumer.Workers.DoorEventConsumer] Consuming event id 5f1d9aa6-1bdb-494b-95a5-bdd2e3d1673b type "DoorOpenEvent" for door "3"
[DBG] [DotNetKafkaSample.Consumer.Workers.DoorEventConsumer] Waiting for next message in topic "door-events"
[DBG] [DotNetKafkaSample.Consumer.Workers.DoorEventConsumer] Consuming "4967308c-9daa-4cf5-9aee-e8fec11c5c7f" from partition 0 Offset 68676 Message "{\"event-type\":\"DoorClosed\",\"event-id\":\"4967308c-9daa-4cf5-9aee-e8fec11c5c7f\",\"event-date-time\":\"2024-08-26T11:02:29.755249Z\",\"door-id\":\"2\"}"
[DBG] [DotNetKafkaSample.Consumer.Workers.DoorEventConsumer] Consuming event id 4967308c-9daa-4cf5-9aee-e8fec11c5c7f type "DoorClosedEvent" for door "2"
[DBG] [DotNetKafkaSample.Consumer.Workers.DoorEventConsumer] Waiting for next message in topic "door-events"
```
You can stop the worker and new door event messages being consumed with `ctrl+c`.

## Running Apache Kafka, the Consumer and Producer inside of Docker

### Building .NET Worker Images

`DockerFile`'s exist in both projects and images can be built using the following commands:

```
docker build -f Producer/Dockerfile -t dotnetkafkatest/door-event-producer  .
docker build -f Consumer/Dockerfile -t dotnetkafkatest/door-event-consumer  .
```

If you now issue a `docker image ls` command you will see that images now exist for both the producer and consumer.  

### Running A Single Kafka Instance Multi-Container Docker Application

There is a `docker-compose.yml` file in the root of the project which will create a multi-container Docker application with:

- A single Apache Kafka instance
- A [Kafka-UI](https://github.com/provectus/kafka-ui) web-based administration console which can be accessed from http://localhost:80
- A Producer .NET worker service which produces messages to the Kafka topic `door-events`
- A Consumer .NET worker service which consumes messages from the Kafka topic `door-events`

You can run this with:

```
docker-compose up
```

If you want to remove the application from your Docker installation you can do this with:

```
docker-compose rm -fv
```

#### Can I Connect To The Kafka Instance From Outside Of The Docker Network ?

Yes you can.  The Kafka instance running in docker exposes itself outside of the docker internal network on port `29092`.   If you want to connect from outside your bootstrap-server needs to be `localhost:29092`.   You can test this with your own application or use the [kcat](https://github.com/edenhill/kcat) tool to do this with something like

```
kcat -b localhost:29092 -L
```

You can even start more instances of the `Producer` worker above locally if you modify the `bootstrapServers` property in `appsettings.json` to `localhost:29092`.  Give it a try - and take a look in `docker-compose.yml` to see how this is achieved - there is specific Kafka config to make this happen, this is not as simple as just exposing the port from the container because of how Kafka advertises broker nodes to its clients.


### Running A Multi-Node Kafka Cluster Multi-Container Docker Application

There is a `docker-compose-with-cluster.yml` file in the root of the project which will create a multi-container Docker application with:

- A 3-node Apache Kafka Cluster
- A [Kafka-UI](https://github.com/provectus/kafka-ui) web-based administration console which can be accessed from http://localhost:80
- A Producer .NET worker service which produces messages to the Kafka topic `door-events`
- A Consumer .NET worker service which consumes messages from the Kafka topic `door-events`

You can run this clustered version of Kafka with:

```
docker-compose -f docker-compose-with-cluster.yml up
```
If you want to remove the application from your Docker installation you can do this with:

```
docker-compose -f docker-compose-with-cluster.yml rm -fv
```

#### Can I Connect To The Kafka Cluster From Outside Of The Docker Network ?

Yes you can.  The Kafka cluster nodes running in docker exposes themselves outside of the docker internal network on ports `29092`, `29093` and `29094`.   If you want to connect from outside your bootstrap-servers needs to be `localhost:29092,localhost:29093,localhost:29094`.   You can test this with your own application or use the [kcat](https://github.com/edenhill/kcat) tool to do this with something like

```
kcat -b localhost:29092,localhost:29093,localhost:29094 -L
```

## Technologies used

- [Serilog](https://serilog.net/) for structured logging
- [Confluent.Kafka](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html) client for access to Kafka

**Note** I have heard great things about [KafkaFlow](https://farfetch.github.io/kafkaflow/) but at present I have not yet experimented with this.

