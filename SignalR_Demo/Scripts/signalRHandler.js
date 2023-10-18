﻿setupConnection = (hubProxy) => {
    hubProxy.client.updatePhoneNumber = (newPhoneNumber) => {
        const telephoneDiv = document.getElementById("telephone");
        telephoneDiv.innerHTML = newPhoneNumber;
    };

    hubProxy.client.expirePhoneNumber = () => {
        const telephoneDiv = document.getElementById("telephone");
        hubProxy.server.generateNewPhoneNumber(telephoneDiv.innerHTML);
        };

    hubProxy.client.finished = (order) => {
        console.log('Finished ${order}');
    }
};


$(document).ready(() => {
    var hubProxy = $.connection.phoneNumberHub;
    setupConnection(hubProxy);
    $.connection.hub.start();
});
