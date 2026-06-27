"use strict";

// aguarda o carregamento completo da pagina
document.addEventListener("DOMContentLoaded", function () {

    // elementos do dom
    const sendButton = document.getElementById("chat-send-btn");
    const messageInput = document.getElementById("chat-message-input");
    const messagesContainer = document.getElementById("messages-container");
    const onlineCounterPage = document.getElementById("chat-online-count");
    const onlineCounterNavbar = document.getElementById("online-count-val");
    const charCounter = document.getElementById("char-counter");
    const typingIndicator = document.getElementById("typing-indicator");
    const typingUsersSpan = document.getElementById("typing-users");

    let currentUser = ''; // nome do utilizador atual
    let typingTimer = null; // timer para controlar a escrita
    let isTyping = false; // indica se o utilizador esta a escrever

    // desativa o botao de envio ate a ligacao ser estabelecida
    if (sendButton) sendButton.disabled = true;

    // funcoes auxiliares

    // escapa caracteres especiais para prevenir xss
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

    // gera uma cor unica para cada utilizador com base no nome
    function getUserColor(username) {
        let hash = 0;
        for (let i = 0; i < username.length; i++) {
            hash = username.charCodeAt(i) + ((hash << 5) - hash);
        }
        const hue = Math.abs(hash) % 360;
        return `hsl(${hue}, 70%, 55%)`;
    }

    // obtem a inicial do nome do utilizador para o avatar
    function getInitials(username) {
        return username.charAt(0).toUpperCase();
    }

    // atualiza o contador de caracteres da mensagem
    function updateCharCounter() {
        if (!charCounter || !messageInput) return;
        const maxLength = 300;
        const currentLength = messageInput.value.length;
        charCounter.textContent = `${currentLength}/${maxLength}`;
        // muda a cor do contador consoante a proximidade do limite
        if (currentLength > maxLength * 0.9) {
            charCounter.style.color = '#e74c3c'; // vermelho
        } else if (currentLength > maxLength * 0.7) {
            charCounter.style.color = '#f39c12'; // laranja
        } else {
            charCounter.style.color = '#6c757d'; // cinza
        }
    }

    // insere um emoji na posicao do cursor
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

    // expoe a funcao insertemoji globalmente para os botoes
    window.insertEmoji = insertEmoji;

    // criar ligacao ao hub
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/kpopChatHub") // endpoint do hub
        .withAutomaticReconnect() // tenta reconectar automaticamente
        .build();

    // ouvir eventos do servidor

    // evento: receber mensagem
    connection.on("ReceiveMessage", function (user, message, timestamp) {
        const cleanUser = escapeHtml(user);
        const cleanMessage = escapeHtml(message);
        const color = getUserColor(user);
        const initials = getInitials(user);

        // determina se a mensagem e do utilizador atual
        const normalizedCurrentUser = currentUser.toLowerCase();
        const normalizedUser = user.toLowerCase();
        const isMe = normalizedCurrentUser === normalizedUser;

        // cria o elemento da mensagem
        const messageDiv = document.createElement("div");
        messageDiv.className = `chat-message-wrapper animate-fade-in ${isMe ? 'my-message' : 'other-message'}`;

        // processa mencoes na mensagem (@nome)
        let processedMessage = cleanMessage;
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

        // constroi o html da mensagem
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

        // adiciona a mensagem ao container
        messagesContainer.appendChild(messageDiv);
        // faz scroll para a ultima mensagem
        messagesContainer.scrollTop = messagesContainer.scrollHeight;

        // notificacao de nova mensagem (se a pagina nao estiver ativa)
        if (document.hidden && !isMe) {
            document.title = `💬 Nova mensagem de ${user} - K-Shelf Chat`;
        }
    });

    // evento: atualizar contador online
    connection.on("UpdateOnlineCount", function (count) {
        if (onlineCounterPage) {
            onlineCounterPage.textContent = count;
        }
        if (onlineCounterNavbar) {
            onlineCounterNavbar.textContent = count;
        }
    });

    // evento: mensagem do sistema (entrada/saida)
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

    // evento: indicador "a escrever..."
    connection.on("UserTyping", function (user) {
        if (user.toLowerCase() !== currentUser.toLowerCase()) {
            typingUsersSpan.textContent = `${user} está a escrever`;
            typingIndicator.style.display = 'block';
        }
    });

    // evento: parou de escrever
    connection.on("UserStoppedTyping", function (user) {
        if (user.toLowerCase() !== currentUser.toLowerCase()) {
            typingUsersSpan.textContent = '';
            typingIndicator.style.display = 'none';
        }
    });

    // iniciar ligacao

    // obtem o nome do utilizador atual (para comparacao)
    const userHeader = document.querySelector(".navbar-k-shelf .nav-link.text-white");
    if (userHeader) {
        let userText = userHeader.textContent.trim();
        // remove "Ola " e "!" se existirem
        userText = userText.replace(/^Olá\s*/, "").replace(/!$/, "").trim();
        // se for email, usa a parte antes do @
        if (userText.includes("@")) {
            currentUser = userText.split('@')[0];
        } else {
            currentUser = userText;
        }
    }

    // fallback: se nao encontrar, usa "anonimo"
    if (!currentUser) {
        currentUser = "Anónimo";
    }

    console.log("Utilizador atual:", currentUser);

    // inicia a ligacao ao hub
    connection.start().then(function () {
        if (sendButton) sendButton.disabled = false;
        console.log("SignalR: Ligação bem sucedida ao KpopChatHub!");

        // notifica a entrada no chat
        connection.invoke("UserJoined", currentUser);

    }).catch(function (err) {
        console.error("SignalR: Falha ao ligar ao Hub: " + err.toString());
    });

    // eventos da interface

    // contador de caracteres
    if (messageInput) {
        messageInput.addEventListener('input', function () {
            updateCharCounter();

            // detecta se o utilizador esta a escrever
            if (this.value.length > 0 && !isTyping) {
                isTyping = true;
                connection.invoke("SendTyping", currentUser);
            } else if (this.value.length === 0 && isTyping) {
                isTyping = false;
                connection.invoke("StopTyping", currentUser);
            }

            // limpa o timer para parar de escrever
            clearTimeout(typingTimer);
            typingTimer = setTimeout(() => {
                if (isTyping && this.value.length === 0) {
                    isTyping = false;
                    connection.invoke("StopTyping", currentUser);
                }
            }, 1500);
        });
    }

    // envia mensagem ao pressionar enter
    const chatForm = document.getElementById("chat-form");
    if (chatForm) {
        chatForm.addEventListener("submit", function (event) {
            event.preventDefault(); // evita o reload da pagina
            sendMessageAction();
        });
    }

    // funcao que envia a mensagem
    function sendMessageAction() {
        const messageText = messageInput.value.trim();
        if (messageText === "") return;

        // chama o metodo do servidor hub sendmessage
        connection.invoke("SendMessage", messageText).then(function () {
            messageInput.value = "";
            messageInput.focus();
            updateCharCounter();

            // para o indicador de escrita
            if (isTyping) {
                isTyping = false;
                connection.invoke("StopTyping", currentUser);
            }
        }).catch(function (err) {
            console.error("SignalR Erro de Envio: " + err.toString());
        });
    }

    // reconexao

    // evento: a tentar reconectar
    connection.onreconnecting(function (error) {
        console.log("SignalR: A tentar reconectar...");
        if (sendButton) sendButton.disabled = true;
    });

    // evento: reconectado com sucesso
    connection.onreconnected(function (connectionId) {
        console.log("SignalR: Reconectado! ID: " + connectionId);
        if (sendButton) sendButton.disabled = false;
        // notifica que o utilizador esta de volta
        connection.invoke("UserJoined", currentUser);
    });

    // evento: ligacao fechada
    connection.onclose(function (error) {
        console.log("SignalR: Ligação fechada.");
        if (sendButton) sendButton.disabled = true;
    });

    // restaurar titulo quando a pagina fica ativa
    document.addEventListener('visibilitychange', function () {
        if (!document.hidden) {
            document.title = 'Chat Global K-Pop - K-Shelf';
        }
    });

    // inicializar contador
    updateCharCounter();

    console.log('Chat inicializado com sucesso!');
});