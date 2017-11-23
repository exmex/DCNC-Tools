{{ $answerIds }}

> {{ $packetDescription }}

## Packet Informations

| Information   |                      |
| ------------- | -------------------- |
{{ $packetInformations }}

## Packet Structure

| Data        | Type             | Size             | Description                  |
| ----------- | ---------------- | ---------------- | ---------------------------- |
{{ $packetStructure }}

## HexDump of Packet

| Hex (Size: {{ $packetHexDumpSize }} Bytes)                              |                    |
| ------------------------------------------------- | ------------------ |
{{ $packetHexDump }}

{{ $comment }}