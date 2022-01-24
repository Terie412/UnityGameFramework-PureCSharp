@echo off

SET OutputDir= output

if not exist %OutputDir% (
	mkdir %OutputDir%
)

protoc --proto_path=src --csharp_out=output protocol_all.proto
copy .\output\ProtocolAll.cs ..\server\GameServer\GameServer\ProtocolAll.cs
copy .\output\ProtocolAll.cs ..\client\Assets\Scripts\Net\ProtocolAll.cs