const URL_API = 'https://localhost:7081/api/v1/Carteira';
let modoEdicao = false;

document.addEventListener("DOMContentLoaded", listarCarteiras);

async function listarCarteiras() {
    const response = await fetch(URL_API);
    const carteiras = await response.json();
    const corpo = document.getElementById('corpoTabela');
    corpo.innerHTML = '';

    carteiras.forEach(c => {
        corpo.innerHTML += `
            <tr>
                <td>${c.numeroCarteira}</td>
                <td>${c.nomeCarteira}</td>
                <td>${c.apetiteCarteira.toFixed(2)}</td>
                <td>
                    <button class="btn-editar" onclick="prepararEdicao(${c.numeroCarteira}, '${c.nomeCarteira}', ${c.apetiteCarteira})">Editar</button>
                    <button class="btn-excluir" onclick="excluirCarteira(${c.numeroCarteira})">Excluir</button>
                </td>
            </tr>`;
    });
}

async function salvar() {
    const num = document.getElementById('numCarteira').value;
    const nome = document.getElementById('nomeCarteira').value;
    const apetite = document.getElementById('apetite').value;

    // Validação obrigatória: só salva se todos os campos estiverem preenchidos
    if (!num || !nome || !apetite) {
        alert("Preencha todos os campos antes de prosseguir.");
        return;
    }

    const carteira = { 
        numeroCarteira: parseInt(num), 
        nomeCarteira: nome, 
        apetiteCarteira: parseFloat(apetite) 
    };
    
    const metodo = modoEdicao ? 'PUT' : 'POST';
    const urlFinal = modoEdicao ? `${URL_API}/${num}` : URL_API;

    const response = await fetch(urlFinal, {
        method: metodo,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(carteira)
    });

    if (response.ok) {
        alert(modoEdicao ? "Carteira atualizada!" : "Carteira cadastrada!");
        limparCampos();
        listarCarteiras();
    }
}

function prepararEdicao(num, nome, apetite) {
    document.getElementById('numCarteira').value = num;
    document.getElementById('numCarteira').disabled = true;
    document.getElementById('nomeCarteira').value = nome;
    document.getElementById('apetite').value = apetite;
    modoEdicao = true;
}

async function excluirCarteira(num) {
    // Caixa de diálogo de confirmação solicitada
    if (confirm(`Deseja realmente excluir a carteira ${num}?`)) {
        const response = await fetch(`${URL_API}/${num}`, { method: 'DELETE' });
        if (response.ok) {
            listarCarteiras(); // Atualiza a visualização e remove da memória RAM
        }
    }
}

function limparCampos() {
    document.getElementById('numCarteira').value = '';
    document.getElementById('numCarteira').disabled = false;
    document.getElementById('nomeCarteira').value = '';
    document.getElementById('apetite').value = '';
    modoEdicao = false;
}