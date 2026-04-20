//Formata um número para moeda brasileira (BRL)
function formatCurrency(value) {
    return value.toLocaleString('pt-BR', {
        style: 'currency',
        currency: 'BRL'
    });
}

let items = [];
let isSubmitting = false;

document.addEventListener('DOMContentLoaded', function () {
    handleSuccessMessage();
    bindProductSelect();
    bindAddItem();
    bindFormSubmit();
    renderItems();
    updateTotal();
});

//Exibe mensagem de sucesso (TempData) e remove após alguns segundos
function handleSuccessMessage() {
    const msg = document.getElementById('success-message');
    if (!msg) return;

    setTimeout(() => {
        msg.classList.add('opacity-0');
        setTimeout(() => msg.remove(), 500);
    }, 4000);
}

//Atualiza automaticamente o valor unitário ao selecionar um produto
function bindProductSelect() {
    const productSelect = document.getElementById("productSelect");
    if (!productSelect) return;

    productSelect.addEventListener("change", function () {
        const selected = this.options[this.selectedIndex];
        const price = selected.getAttribute("data-price");

        document.getElementById("unitPrice").value =
            price ? formatCurrency(Number(price)) : "";
    });
}

//Atualiza automaticamente o valor unitário ao selecionar um produto
function bindAddItem() {
    const btn = document.getElementById("btnAddItem");
    if (!btn) return;

    btn.addEventListener("click", addItem);
}

//Adiciona um item à lista com validações básicas e atualiza a UI
function addItem() {
    clearMessage();
    const productSelect = document.getElementById("productSelect");

    const productId = parseInt(productSelect.value, 10);
    const selectedOption = productSelect.options[productSelect.selectedIndex];
    const productText = selectedOption ? selectedOption.text : "";
    const price = parseFloat(selectedOption?.dataset.price || 0);

    const quantityInput = document.querySelector("[name='InputItem.Quantity']");
    const quantity = parseInt(quantityInput.value || 0, 10);

    if (!productId || quantity <= 0) {
        showMessage("Selecione produto e quantidade", "warning");
        return;
    }

    if (price <= 0) {
        showMessage("Produto inválido", "error");
        return;
    }

    //Evita duplicado
    if (items.some(i => i.ProductId === productId)) {
        showMessage("Produto já adicionado", "error");
        return;
    }

    const item = {
        ProductId: productId,
        ProductName: productText,
        Quantity: quantity,
        UnitPrice: price,
        Subtotal: price * quantity
    };

    items.push(item);

    showMessage("Item adicionado com sucesso", "success");

    renderItems();
    updateTotal();
    syncHiddenField();
    resetForm();
}

//Renderiza os itens adicionados na tabela
function renderItems() {
    const tbody = document.querySelector("table tbody");
    if (!tbody) return;

    if (items.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center p-4 text-gray-400">
                    Nenhum item adicionado
                </td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = items.map((item, index) => `
        <tr>
            <td class="p-2">${item.ProductName}</td>
            <td class="text-center p-2">${item.Quantity}</td>
            <td class="text-right p-2">${formatCurrency(item.UnitPrice)}</td>
            <td class="text-right p-2 font-medium">${formatCurrency(item.Subtotal)}</td>
            <td class="text-center p-2">
                <button type="button" onclick="removeItem(${index})" class="link--danger text-xs">
                    Remover
                </button>
            </td>
        </tr>
    `).join("");
}

//Remove um item da lista e atualiza a UI
function removeItem(index) {
    clearMessage();
    items.splice(index, 1);
    renderItems();
    updateTotal();
    syncHiddenField();
}

//Calcula e exibe o valor total do orçamento
function updateTotal() {
    const total = items.reduce((sum, i) => sum + i.Subtotal, 0);

    const el = document.getElementById("totalAmount");
    if (el) el.innerText = formatCurrency(total);
}

//Sincroniza os itens com o campo hidden (JSON enviado ao backend)
function syncHiddenField() {
    const hidden = document.getElementById("itemsJson");
    if (hidden) {
        hidden.value = JSON.stringify(items);
    }
}

//Reseta os campos do formulário de item (produto, quantidade, preço)
function resetForm() {
    const productSelect = document.getElementById("productSelect");

    productSelect.value = "";
    productSelect.dispatchEvent(new Event("change"));

    document.querySelector("[name='InputItem.Quantity']").value = "";
    document.getElementById("unitPrice").value = "";
}

let messageTimeout;

//Exibe uma mensagem no formulário (erro, sucesso ou aviso)
function showMessage(message, type = "error") {
    const container = document.getElementById("form-message");
    if (!container) return;

    const styles = {
        error: "bg-red-50 text-red-600",
        success: "bg-green-50 text-green-600",
        warning: "bg-yellow-50 text-yellow-600"
    };

    container.className = "mb-4 p-3 rounded-lg text-sm";

    container.classList.remove(
        "bg-red-50", "text-red-600",
        "bg-green-50", "text-green-600",
        "bg-yellow-50", "text-yellow-600"
    );

    container.classList.add(...styles[type].split(" "));

    container.innerText = message;
    container.classList.remove("hidden");

    clearTimeout(messageTimeout);

    messageTimeout = setTimeout(() => {
        container.classList.add("hidden");
    }, 4000);
}

//Oculta a mensagem atual do formulário
function clearMessage() {
    const container = document.getElementById("form-message");
    if (container) container.classList.add("hidden");
}

//Controla o envio do formulário, evitando submit duplo e validando itens
function bindFormSubmit() {
    const form = document.querySelector("form");
    if (!form) return;

    form.addEventListener("submit", function (e) {

        if (isSubmitting) {
            e.preventDefault();
            return;
        }

        if (items.length === 0) {
            e.preventDefault();
            showMessage("Adicione pelo menos um item ao orçamento", "error");
            isSubmitting = false;
            return;
        }

        isSubmitting = true;

        const submitButton = form.querySelector("button[type='submit']");
        if (submitButton) {
            submitButton.disabled = true;
            submitButton.classList.add("opacity-50", "cursor-not-allowed");
            submitButton.innerText = "Salvando...";
        }
    });
}