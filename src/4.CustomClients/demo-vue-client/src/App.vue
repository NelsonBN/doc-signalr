<template>
    <h1>Vue Client</h1>
</template>

<script lang="ts">
    import { defineComponent } from 'vue';
    import * as signalR from '@microsoft/signalr';

    export default defineComponent({
        name: 'App',
        components: {}
    });

    const connection = new signalR.HubConnectionBuilder()
		.withUrl("http://localhost:8080/my-hub")
		.configureLogging(signalR.LogLevel.Information)
		.withAutomaticReconnect([0, 500, 1000, 2000, 5000, 10000, 30000, 60000])
		.build();

    connection.onclose(() => console.log("[SIGNALR] Disconnected from server"));
    connection.on("Pong", data => console.log('[RETURN MESSAGE]:', data));

    try {
        connection
        .start()
        .then(() => {
            console.log("[SIGNALR] Connected to server");
            connection.send('Ping', "Hello from vue client");

            sendMessage();
        });
    } catch (err) {
        console.error("[ERROR]", err);
    }

    const sendMessage = () => {
        connection.send('Ping', "Continuing to ping from vue client");
        setTimeout(sendMessage, 1000);
    };
</script>
