#build
FROM maven:3.9-eclipse-temurin-21 AS build

WORKDIR /app

#copy maven archives
COPY pom.xml .
COPY app/pom.xml app/
COPY test-client/pom.xml test-client/

        #download dependences
RUN mvn dependency:go-offline -pl app -am -q

#generate JAR
COPY app/src app/src
RUN mvn -pl app package -am -DskipTests -q

#run
FROM eclipse-temurin:21-jre-alpine AS run

WORKDIR /app

#copy JAR
COPY --from=build /app/app/target/*.jar app.jar

#port
EXPOSE 8080

ENTRYPOINT ["java", "-jar", "app.jar"]
