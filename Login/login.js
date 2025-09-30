const nomeInput = document.getElementById("username-input");
const senhaInput = document.getElementById("password-input");
const emailInput = document.getElementById("email-input");

const nomeErrorLabel = document.getElementById("error-username");
const senhaErrorLabel = document.getElementById("error-password");
const emailErrorLabel = document.getElementById("error-email");


const minLengthNome = 6;
const maxLengthNome = 40;

const minLengthSenha = 6;
const maxLengthSenha = 20;

const minLengthEmail = 11;
const maxLengthEmail = 40;


const serverRoute = "http://localhost:8080";



let nome;
let senha;
let email;

const emailRegex = /^[a-zA-Z0-9._+%-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;


const validarEntradas = () => {
    let n = nomeInput.value;
    let s = senhaInput.value;
    let e = emailInput.value;

    let c = 0;

    if (n.length >= minLengthNome && n.length <= maxLengthNome && !n.includes(" ")){
        nome = n;
        nomeErrorLabel.textContent = "";
        
        c++;

        // c++ kk
    }
    else{
        nomeErrorLabel.textContent = `O nome de usuário deve conter entre ${minLengthNome} e ${maxLengthNome} caractéres e não conter espaços!`;
    }

    if (s.length >= minLengthSenha && s.length <= maxLengthSenha && !s.includes(' ')){
        senha = s;
        senhaErrorLabel.textContent = "";
        
        c++;
    }
    else{
        senhaErrorLabel.textContent = `A senha deve conter entre ${minLengthSenha} e ${maxLengthSenha} caractéres e não conter espaços!`;
    }


    
    if (e.length >= minLengthEmail && e.length <= maxLengthEmail && emailRegex.test(e)){
        email = e;
        emailErrorLabel.textContent = '';

        c++
    }
    else{
        emailErrorLabel.textContent = 'Endereço de email inválido!'
    }


    return c == 3;
}




const verificarUsuario = async () => {
    if (validarEntradas()){
        try {
            const resposta = await fetch(`${serverRoute}/usuarios?nome=${encodeURIComponent(nome)}&senha=${encodeURIComponent(senha)}&email=${encodeURIComponent(email)}`, {
                method: 'GET',
                headers: {"Content-Type": "application/json"},
            });

            const dados = await resposta.json();
            
            return dados.mensagem === true;

        } catch (error){
            console.error("Erro:", error);
        }
    } else{
        return null;
    }
}



const acessarConta = async () => {
    const usuarioExiste = await verificarUsuario();

    if (usuarioExiste === true){
        sessionStorage.setItem('usuario', nome);
        sessionStorage.setItem('senha', senha);
        sessionStorage.setItem('email', email);

        window.open(`../Page1/page1.html`, '_self');
    } else if (usuarioExiste === null){
        console.error("Algo deu errado ao verificar a existência do usuário");
    } else {
        alert(`Este usuário não existe!`);
    }
}