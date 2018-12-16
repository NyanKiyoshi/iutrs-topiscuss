# Change logs

## v1.4
- Implemented the below commands:

  | Command      | Description  |
  |--------------|--------------|
  | `QUIT      ` | Closes the client (client command). |
  | `HELP      ` | Retrieve the list of available commands (client command). |
  | `EXIT      ` | Closes and removes the current server room's connection. |
  | `CREATEROOM` | Creates a new room in a given server, which opens a new port. |
  | `LISTROOMS ` | Lists the available rooms of the currently connected server (see the `CONNECT` command).             |
  | `CONNECT   ` | Connect to a given `IP[:port]` combination. |
  
- Refactored the static server for an instantiable server class, 
  allowing more flexibility;
- Fixed a bug where if the client's listening `IPEndpoint` 
  was set to a non-routable IP, it would crash 
  (fixes [#9](https://github.com/NyanKiyoshi/iutrs-topiscuss/issues/9)).

## v1.3
- Fix the server's handling of non properly closed client connections,
  closes [#6](https://github.com/NyanKiyoshi/iutrs-topiscuss/issues/6);
- Doing a CTRL+Z on the message input, no longer crashes the client;
- Requesting the client to kill itself, now properly closes
  the client socket. C# is not climbing back the stack or cleaning
  disposables when getting killed. This case is now handled;
- No longer a allow a full command name input, to allow the user
  to quickly send a wanted command, in only one key,
  not two (e.g.: 0 + Return);
- The default endpoint configuration is now a method,
  which is the proper, and clean way to do;
- The argument parsing, now handles (again) empty arguments and, thus,
  Main() does not have to check anything anymore. Making the code
  cleaner;
- No longer logging whenever an issue occurred while receiving a message
  from the client socket (e.g. timeout). This could,
  and may will be replaced later by a server status
  feedback on the client;
- Simplified the client's SendMessage, making it cleaner;
- The whole client can now be thread safe and cleanly independent,
  as the client is now a object that can get destroyed without exiting
  the application, or depending on the application.
 
## v1.2
- [x] `SUB` is adding requesters endpoint into the server;
- [x] `SUB` does nothing if the request's endpoint is **already** subbed;
- [x] `UNSUB` is removing the requester's endpoint;
- [x] `UNSUB` does nothing if the request's endpoint is **not yet** subbed;
- [x] `POST` command is sending the saved message to the subscribers;
- [x] The client now waits for messages (<1ms) after any command, and from the start until the user requests to stop looking for messages;
- [x] The changes are tested, and, covered;
- [x] The changes are fully commented. 
 
## v1.1
- [x] `POST` is storing the received message into the server;
- [x] `GET` is sending every stored messages;
- [x] The client does not block the user's input for too long when waiting for messages (<1ms);
- [x] The changes are tested, and, covered.
  
## v1.0
- [x] The serialization method `public byte[] GetBytes()` is implemented;
- [x] The de-serialization construction method `public ChatMessage(byte[] buffer)` is implemented;
- [x] The `ChatMessage` has a mandatory `nickname` field into the buffer;
- [x] Invalid sizes are handled (fields, and the overall buffer).
