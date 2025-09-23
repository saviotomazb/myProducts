const inputs = document.querySelectorAll(".code-input");
const errorMessage = document.getElementById("errorMessage");

// Função para juntar os valores dos inputs
function getCode() {
    return Array.from(inputs).map(input => input.value).join("");
}

// Apenas retorna se todos os inputs estão preenchidos
function isComplete() {
    return getCode().length === inputs.length;
}

// Avança os inputs automaticamente e trata backspace e paste
inputs.forEach((input, index) => {
    input.addEventListener("input", () => {
        if (input.value.length === 1 && index < inputs.length - 1) {
            inputs[index + 1].focus();
        }
    });

    input.addEventListener("keydown", (e) => {
        if (e.key === "Backspace" && !input.value && index > 0) {
            inputs[index - 1].focus();
        }
    });

    input.addEventListener("paste", (e) => {
        e.preventDefault();
        const pasteData = (e.clipboardData || window.clipboardData).getData("text");
        const chars = pasteData.split("");

        chars.forEach((char, i) => {
            if (index + i < inputs.length) {
                inputs[index + i].value = char;
            }
        });

        const lastIndex = Math.min(index + chars.length - 1, inputs.length - 1);
        inputs[lastIndex].focus();
    });
});

// Mostrar mensagem de erro apenas ao tentar enviar o formulário
document.querySelector("form").addEventListener("submit", function (e) {
    if (!isComplete()) {
        e.preventDefault();
        errorMessage.classList.remove("hidden"); // Exibe mensagem de erro
    } else {
        errorMessage.classList.add("hidden"); // Oculta a mensagem se estiver correto
    }
});