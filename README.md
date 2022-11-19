# NATS Request/Reply Sample
- Demostrates NATS messaging with browser. Request and reply clients are located in browser.
- Use docker compose to start nats server with WebSocket (WS) enabled.
- nats.js is included with code. To update it run `npm install nats.ws`. And see the `\node_modules\nats.ws\esm` folder. Copy the new nats.js to the `/js` folder.
## Asp.Net Request Client
- While request client is in Asp.Net, reply client is in browser and accepts messages with WebSocket (WS).
- See `NatsService.cs` for usage of Nats connection.
