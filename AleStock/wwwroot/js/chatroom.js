"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.start()

//Disable the send button until connection is established.
// document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMsg", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${user}: ${message}`;
});

connection.on("SendMsg", function (message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${message}`;
});

//connection.start().then(function () {
//    document.getElementById("sendButton").disabled = false;
//}).catch(function (err) {
//    return console.error(err.toString());
//});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMsg", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

//connection.on("SendPrivateMsg", function (user, targetConnectionId, message) {
//    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
//    var encodedMsg = user + "(PM): " + msg + " by " + targetConnectionId;
//    var li = document.createElement("li");
//    li.textContent = encodedMsg;
//    document.getElementById("messagesList").appendChild(li);
//});

//// function for sending messages to group
//document.getElementById("groupmsg").addEventListener("click", async (event) => {
//    var groupName = document.getElementById("group-name").value;
//    var userName = document.getElementById("user-name").value;
//    var groupMsg = document.getElementById("group-message-text").value;
//    try {
//        connection.invoke("SendMsgToGroup", groupName, groupMsg, userName);
//    }
//    catch (e) {
//        console.error(e.toString());
//    }
//    event.preventDefault();
//});

//// function for joining a group
//document.getElementById("join-group").addEventListener("click", async (event) => {
//    var groupName = document.getElementById("group-name").value;
//    var userName = document.getElementById("user-name").value;
//    try {
//        connection.invoke("AddToGroup", groupName, userName);
//    }
//    catch (e) {
//        console.error(e.toString());
//    }
//    event.preventDefault();
//});

//// function for leaving a group
//document.getElementById("leave-group").addEventListener("click", async (event) => {
//    var groupName = document.getElementById("group-name").value;
//    try {
//        connection.invoke("RemoveFromGroup", groupName);
//    }
//    catch (e) {
//        console.error(e.toString());
//    }
//    event.preventDefault();
//});