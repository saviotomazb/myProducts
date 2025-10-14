const form = document.querySelector("form");
const inputs = document.querySelectorAll(".code-input");
const hiddenCode = document.getElementById("hiddenCode");
const validationSpan = document.querySelector('[asp-validation-for="Input.Code"]');

// Função para juntar os valores dos inputs
function getCode() {
    return Array.from(inputs).map(input => input.value).join("");
}

// Atualiza o campo oculto
function updateHiddenCode() {
    hiddenCode.value = getCode();
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
        updateHiddenCode();
    });

    input.addEventListener("keydown", (e) => {
        if (e.key === "Backspace" && !input.value && index > 0) {
            inputs[index - 1].focus();
        }
        updateHiddenCode();
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

        updateHiddenCode();
    });
});

// Validação antes do submit
form.addEventListener("submit", (e) => {
    updateHiddenCode();
    if (!isComplete()) {
        e.preventDefault();
        validationSpan.textContent = "Preencha todos os 5 dígitos";
    } else {
        validationSpan.textContent = "";
    }
});