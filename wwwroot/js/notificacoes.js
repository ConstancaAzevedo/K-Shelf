"use strict";

// sistema de notificações em tempo real com SignalR

let notificacaoConnection = null;

function iniciarNotificacoes() {
    if (typeof signalR === 'undefined') {
        console.warn('SignalR não está carregado.');
        return;
    }

    if (notificacaoConnection && notificacaoConnection.state === signalR.HubConnectionState.Connected) {
        console.log('Notificações: Conexão já ativa.');
        return;
    }

    console.log('Notificações: A iniciar conexão...');

    notificacaoConnection = new signalR.HubConnectionBuilder()
        .withUrl("/notificacaoHub")
        .withAutomaticReconnect()
        .build();

    notificacaoConnection.on("ReceberNotificacao", function (notificacao) {
        console.log('Notificação recebida:', notificacao);
        mostrarToast(notificacao);
    });

    notificacaoConnection.start()
        .then(function () {
            console.log('Notificações: Conectado com sucesso!');
        })
        .catch(function (err) {
            console.error('Notificações: Erro ao conectar:', err);
            setTimeout(iniciarNotificacoes, 5000);
        });
}

function mostrarToast(notificacao) {
    const toastContainer = document.getElementById('toast-container') || criarToastContainer();

    const toast = document.createElement('div');
    toast.className = `toast-notification animate-slide-in`;
    toast.innerHTML = `
        <div class="toast-header">
            <span class="toast-icon">🔔</span>
            <strong class="toast-title">${notificacao.Tipo}</strong>
            <small class="toast-time ms-auto">${new Date(notificacao.Data).toLocaleTimeString('pt-PT')}</small>
            <button type="button" class="btn-close" onclick="this.closest('.toast-notification').remove()"></button>
        </div>
        <div class="toast-body">
            ${notificacao.Mensagem}
        </div>
    `;

    toastContainer.appendChild(toast);

    setTimeout(function () {
        toast.classList.add('animate-slide-out');
        setTimeout(function () { toast.remove(); }, 300);
    }, 5000);
}

function criarToastContainer() {
    const container = document.createElement('div');
    container.id = 'toast-container';
    container.style.cssText = `
        position: fixed;
        top: 80px;
        right: 20px;
        z-index: 9999;
        display: flex;
        flex-direction: column;
        gap: 10px;
        max-width: 400px;
        width: 100%;
        pointer-events: none;
    `;
    document.body.appendChild(container);
    return container;
}

// Iniciar automaticamente
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(iniciarNotificacoes, 1000);
});

// Estilos dos toasts
const estiloToasts = document.createElement('style');
estiloToasts.textContent = `
    #toast-container .toast-notification {
        pointer-events: auto;
        background: rgba(255, 255, 255, 0.95);
        backdrop-filter: blur(10px);
        border-radius: 12px;
        box-shadow: 0 8px 32px rgba(0, 0, 0, 0.15);
        border: 1px solid rgba(255, 255, 255, 0.2);
        overflow: hidden;
        min-width: 300px;
        max-width: 400px;
        animation: slideIn 0.4s cubic-bezier(0.25, 0.46, 0.45, 0.94) forwards;
    }
    #toast-container .toast-header {
        padding: 10px 15px;
        display: flex;
        align-items: center;
        gap: 8px;
        border-bottom: 1px solid rgba(0, 0, 0, 0.05);
    }
    #toast-container .toast-body {
        padding: 12px 15px;
        font-size: 0.9rem;
        color: #2d3436;
    }
    .animate-slide-out {
        animation: slideOut 0.3s cubic-bezier(0.55, 0.085, 0.68, 0.53) forwards;
    }
    @@keyframes slideIn {
        from { opacity: 0; transform: translateX(100%) scale(0.9); }
        to { opacity: 1; transform: translateX(0) scale(1); }
    }
    @@keyframes slideOut {
        from { opacity: 1; transform: translateX(0) scale(1); }
        to { opacity: 0; transform: translateX(100%) scale(0.9); }
    }
`;
document.head.appendChild(estiloToasts);