"use strict";

document.addEventListener("DOMContentLoaded", function () {

    // ELEMENTOS DOM
    const sendButton = document.getElementById("chat-send-btn");
    const messageInput = document.getElementById("chat-message-input");
    const messagesContainer = document.getElementById("messages-container");
    const onlineCounterPage = document.getElementById("chat-online-count");
    const onlineCounterNavbar = document.getElementById("online-count-val");
    const charCounter = document.getElementById("char-counter");
    const typingIndicator = document.getElementById("typing-indicator");
    const typingUsersSpan = document.getElementById("typing-users");

    let currentUser = '';
    let typingTimer = null;
    let isTyping = false;

    // Desativar botão de envio até que a ligação seja estabelecida
    if (sendButton) sendButton.disabled = true;

    // funções auxiliares

    function escapeHtml(string) {
        if (!string) return '';
        return String(string).replace(/[&<>"'`=\/]/g, function (s) {
            const map = {
                "&": "&amp;",
                "<": "&lt;",
                ">": "&gt;",
                '"': "&quot;",
                "'": "&#39;",
                "/": "&#x2F;",
                "`": "&#x60;",
                "=": "&#x3D;"
            };
            return map[s] || s;
        });
    }

    function getUserColor(username) {
        let hash = 0;
        for (let i = 0; i < username.length; i++) {
            hash = username.charCodeAt(i) + ((hash << 5) - hash);
        }
        const hue = Math.abs(hash) % 360;
        return `hsl(${hue}, 70%, 55%)`;
    }

    function getInitials(username) {
        return username.charAt(0).toUpperCase();
    }

    function updateCharCounter() {
        if (!charCounter || !messageInput) return;
        const maxLength = 300;
        const currentLength = messageInput.value.length;
        charCounter.textContent = `${currentLength}/${maxLength}`;
        if (currentLength > maxLength * 0.9) {
            charCounter.style.color = '#e74c3c';
        } else if (currentLength > maxLength * 0.7) {
            charCounter.style.color = '#f39c12';
        } else {
            charCounter.style.color = '#6c757d';
        }
    }

    function insertEmoji(emoji) {
        if (!messageInput) return;
        const start = messageInput.selectionStart;
        const end = messageInput.selectionEnd;
        const text = messageInput.value;
        messageInput.value = text.substring(0, start) + emoji + text.substring(end);
        messageInput.focus();
        messageInput.selectionStart = messageInput.selectionEnd = start + emoji.length;
        updateCharCounter();
    }

    // Expor a função insertEmoji globalmente para os botões
    window.insertEmoji = insertEmoji;

    // criar ligação ao HUB

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/kpopChatHub")
        .withAutomaticReconnect()
        .build();

    // ouvir eventos do servidor

    // Receber mensagem
    connection.on("ReceiveMessage", function (user, message, timestamp) {
        const cleanUser = escapeHtml(user);
        const cleanMessage = escapeHtml(message);
        const color = getUserColor(user);
        const initials = getInitials(user);

        // Determinar se a mensagem é do utilizador atual
        const normalizedCurrentUser = currentUser.toLowerCase();
        const normalizedUser = user.toLowerCase();
        const isMe = normalizedCurrentUser === normalizedUser;

        const messageDiv = document.createElement("div");
        messageDiv.className = `chat-message-wrapper animate-fade-in ${isMe ? 'my-message' : 'other-message'}`;

        // Processar menções na mensagem
        let processedMessage = cleanMessage;
        // Destacar menções (@nome)
        const mentionRegex = /@(\w+)/g;
        const matches = cleanMessage.match(mentionRegex);
        if (matches) {
            matches.forEach(match => {
                const username = match.substring(1);
                const mentionColor = getUserColor(username);
                const highlighted = `<span class="mention" style="color: ${mentionColor}; font-weight: bold; background: rgba(0,0,0,0.05); padding: 0 4px; border-radius: 4px;">${match}</span>`;
                processedMessage = processedMessage.replace(match, highlighted);
            });
        }

        messageDiv.innerHTML = `
            <div class="d-flex align-items-start gap-2 ${isMe ? 'flex-row-reverse' : ''}">
                <div class="avatar-circle" style="width: 36px; height: 36px; border-radius: 50%; background: ${color}; color: white; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 0.9rem; flex-shrink: 0;">
                    ${initials}
                </div>
                <div style="max-width: 80%;">
                    <div class="message-info ${isMe ? 'text-end' : 'text-start'}">
                        <span class="fw-bold" style="color: ${color};">${isMe ? 'Eu' : cleanUser}</span>
                        <span class="text-muted" style="font-size: 0.7rem;"> • ${escapeHtml(timestamp)}</span>
                    </div>
                    <div class="message-content" style="background: ${isMe ? 'linear-gradient(135deg, var(--kpop-pink), var(--kpop-purple))' : 'white'}; color: ${isMe ? 'white' : 'var(--kpop-dark)'}; ${!isMe ? 'border: 1px solid rgba(0,0,0,0.08);' : ''}">
                        ${processedMessage}
                    </div>
                </div>
            </div>
        `;

        messagesContainer.appendChild(messageDiv);
        messagesContainer.scrollTop = messagesContainer.scrollHeight;

        // Notificação de nova mensagem (se a página não estiver ativa)
        if (document.hidden && !isMe) {
            document.title = `💬 Nova mensagem de ${user} - K-Shelf Chat`;
        }
    });

    // Atualizar contador online
    connection.on("UpdateOnlineCount", function (count) {
        if (onlineCounterPage) {
            onlineCounterPage.textContent = count;
        }
        if (onlineCounterNavbar) {
            onlineCounterNavbar.textContent = count;
        }
    });

    // 3. Mensagem do sistema (entrada/saída)
    connection.on("SystemMessage", function (message) {
        const systemDiv = document.createElement("div");
        systemDiv.className = "text-center my-2 system-message animate-fade-in";
        systemDiv.innerHTML = `
            <span class="badge bg-light text-secondary rounded-pill px-3 py-2 border shadow-sm" style="font-weight: normal;">
                📢 ${escapeHtml(message)}
            </span>
        `;
        messagesContainer.appendChild(systemDiv);
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    });

    // 4. Indicador "a escrever..."
    connection.on("UserTyping", function (user) {
        if (user.toLowerCase() !== currentUser.toLowerCase()) {
            typingUsersSpan.textContent = `${user} está a escrever`;
            typingIndicator.style.display = 'block';
        }
    });

    connection.on("UserStoppedTyping", function (user) {
        if (user.toLowerCase() !== currentUser.toLowerCase()) {
            typingUsersSpan.textContent = '';
            typingIndicator.style.display = 'none';
        }
    });

    // iniciar ligação

    // Obter o nome do utilizador atual (para comparação)
    const userHeader = document.querySelector(".navbar-k-shelf .nav-link.text-white");
    if (userHeader) {
        let userText = userHeader.textContent.trim();
        // Remover "Olá " e "!" se existirem
        userText = userText.replace(/^Olá\s*/, "").replace(/!$/, "").trim();
        // Se for email, usar a parte antes do @
        if (userText.includes("@")) {
            currentUser = userText.split('@')[0];
        } else {
            currentUser = userText;
        }
    }

    // Fallback: se não encontrar, usar "Anónimo" (mas o chat não vai funcionar bem)
    if (!currentUser) {
        currentUser = "Anónimo";
    }

    console.log("Utilizador atual:", currentUser);

    connection.start().then(function () {
        if (sendButton) sendButton.disabled = false;
        console.log("SignalR: Ligação bem sucedida ao KpopChatHub!");

        // Notificar entrada no chat
        connection.invoke("UserJoined", currentUser);

    }).catch(function (err) {
        console.error("SignalR: Falha ao ligar ao Hub: " + err.toString());
    });

    // eventos da interface

    // Contador de caracteres
    if (messageInput) {
        messageInput.addEventListener('input', function () {
            updateCharCounter();

            // Detetar se o utilizador está a escrever
            if (this.value.length > 0 && !isTyping) {
                isTyping = true;
                connection.invoke("SendTyping", currentUser);
            } else if (this.value.length === 0 && isTyping) {
                isTyping = false;
                connection.invoke("StopTyping", currentUser);
            }

            // Limpar o timer para parar de escrever
            clearTimeout(typingTimer);
            typingTimer = setTimeout(() => {
                if (isTyping && this.value.length === 0) {
                    isTyping = false;
                    connection.invoke("StopTyping", currentUser);
                }
            }, 1500);
        });
    }

    // Enviar mensagem (Enter)
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
            updateCharCounter();

            // Parar indicador de escrita
            if (isTyping) {
                isTyping = false;
                connection.invoke("StopTyping", currentUser);
            }
        }).catch(function (err) {
            console.error("SignalR Erro de Envio: " + err.toString());
        });
    }

    // reconexão
    connection.onreconnecting(function (error) {
        console.log("SignalR: A tentar reconectar...");
        if (sendButton) sendButton.disabled = true;
    });

    connection.onreconnected(function (connectionId) {
        console.log("SignalR: Reconectado! ID: " + connectionId);
        if (sendButton) sendButton.disabled = false;
        // Notificar que o utilizador está de volta
        connection.invoke("UserJoined", currentUser);
    });

    connection.onclose(function (error) {
        console.log("SignalR: Ligação fechada.");
        if (sendButton) sendButton.disabled = true;
    });

    // restaurar título quando a página fica ativa
    document.addEventListener('visibilitychange', function () {
        if (!document.hidden) {
            document.title = 'Chat Global K-Pop - K-Shelf';
        }
    });

    // Inicializar contador
    updateCharCounter();

    console.log('Chat inicializado com sucesso!');
});