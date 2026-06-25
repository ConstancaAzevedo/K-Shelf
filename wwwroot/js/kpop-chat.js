"use strict";

document.addEventListener("DOMContentLoaded", function () {
    // 1. Criar a ligação ao Hub do SignalR
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/kpopChatHub")
        .withAutomaticReconnect()
        .build();

    const sendButton = document.getElementById("chat-send-btn");
    const messageInput = document.getElementById("chat-message-input");
    const messagesContainer = document.getElementById("messages-container");
    const onlineCounterPage = document.getElementById("chat-online-count");
    const onlineCounterNavbar = document.getElementById("online-count-val");

    // Desativar botão de envio até que a ligação seja estabelecida
    if (sendButton) sendButton.disabled = true;

    // 2. Ouvir o evento de receção de mensagens do Servidor
    connection.on("ReceiveMessage", function (user, message, timestamp) {
        // Sanitizar a mensagem para evitar ataques XSS
        const cleanUser = escapeHtml(user);
        const cleanMessage = escapeHtml(message);

        // Criar a bolha de mensagem HTML
        const messageDiv = document.createElement("div");
        messageDiv.className = "chat-message-wrapper animate-fade-in";

        // Determinar se a mensagem é do utilizador logado atualmente
        const currentUserHeader = document.querySelector(".navbar-k-shelf .nav-link.text-white");
        let isMe = false;
        if (currentUserHeader) {
            const currentUsername = currentUserHeader.textContent.replace("Olá ", "").replace("!", "").trim();
            const normalizedCurrentUser = currentUsername.includes("@") ? currentUsername.split('@')[0] : currentUsername;
            if (normalizedCurrentUser.toLowerCase() === user.toLowerCase()) {
                isMe = true;
            }
        }

        if (isMe) {
            messageDiv.classList.add("my-message");
            messageDiv.innerHTML = `
                <div class="message-info text-end">
                    <span class="fw-bold">${cleanUser} (Eu)</span> • ${timestamp}
                </div>
                <div class="message-content">
                    ${cleanMessage}
                </div>
            `;
        } else {
            messageDiv.classList.add("other-message");
            messageDiv.innerHTML = `
                <div class="message-info text-start">
                    <span class="fw-bold">${cleanUser}</span> • ${timestamp}
                </div>
                <div class="message-content">
                    ${cleanMessage}
                </div>
            `;
        }

        // Adicionar ao painel e fazer scroll automático para o fundo
        messagesContainer.appendChild(messageDiv);
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    });

    // 3. Ouvir a atualização do contador de utilizadores online
    connection.on("UpdateOnlineCount", function (count) {
        if (onlineCounterPage) {
            onlineCounterPage.textContent = count;
        }
        if (onlineCounterNavbar) {
            onlineCounterNavbar.textContent = count;
        }
    });

    // 4. Iniciar a ligação ao Hub
    connection.start().then(function () {
        if (sendButton) sendButton.disabled = false;
        console.log("SignalR: Ligação bem sucedida ao KpopChatHub!");
    }).catch(function (err) {
        return console.error("SignalR: Falha ao ligar ao Hub: " + err.toString());
    });

    // 5. Enviar Mensagem ao Submeter o Formulário
    const chatForm = document.getElementById("chat-form");
    if (chatForm) {
        chatForm.addEventListener("submit", function (event) {
            event.preventDefault();
            sendMessageAction();
        });
    }

    function sendMessageAction() {
        const messageText = messageInput.value.trim();
        if (messageText === "") return;

        // Chamar o método do servidor Hub SendMessage
        connection.invoke("SendMessage", messageText).then(function () {
            messageInput.value = "";
            messageInput.focus();
        }).catch(function (err) {
            return console.error("SignalR Erro de Envio: " + err.toString());
        });
    }

    // Função utilitária para prevenir injeção de HTML no Chat
    function escapeHtml(string) {
        return String(string).replace(/[&<>"'`=\/]/g, function (s) {
            return {
                "&": "&amp;",
                "<": "&lt;",
                ">": "&gt;",
                '"': "&quot;",
                "'": "&#39;",
                "/": "&#x2F;",
                "`": "&#x60;",
                "=": "&#x3D;"
            }[s];
        });
    }
});
