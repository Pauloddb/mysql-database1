const usuario = sessionStorage.getItem("usuario");
const senha = sessionStorage.getItem("senha");
const email = sessionStorage.getItem('email');


const h2AccountName = document.getElementById('account-name');
h2AccountName.textContent = `Conta: ${usuario}`;


const table = document.getElementById('table');



const serverRoute = "http://177.86.80.108:8080";










const verificarUsuario = async () => {
    try {
        const resposta = await fetch(`${serverRoute}/usuarios?nome=${encodeURIComponent(usuario)}&senha=${encodeURIComponent(senha)}&email=${encodeURIComponent(email)}`, {
            method: 'GET',
            headers: {"Content-Type": "application/json"},
        });

        const dados = await resposta.json();
        
        return dados.mensagem === true;

    } catch (error){
        console.error("Erro:", error);
    }
}



const testarUsuario = async () => {
    let usuarioExiste = await verificarUsuario();

    if (usuarioExiste === false || usuarioExiste === null){
        window.open('../Cadastro/cadastro.html', '_self');
    }
}

testarUsuario();






const updateTable = async () => {
    const resposta = await fetch(`${serverRoute}/getAllBestScores`, {
        method: 'GET',
        headers: {"Content-Type": "application/json"}
    });

    const dados = await resposta.json();

    dados.sort((a, b) => b.bestScore - a.bestScore);



    table.innerHTML = '';

    let firstTr = document.createElement('tr');
    let firstHeader = document.createElement('th');
    let secondHeader = document.createElement('th');
    let thirdHeader = document.createElement('th');

    firstHeader.textContent = 'Posição';
    secondHeader.textContent = 'Usuário';
    thirdHeader.textContent = 'Melhor Pontuação';


    firstTr.append(firstHeader);
    firstTr.append(secondHeader);
    firstTr.append(thirdHeader);

    table.append(firstTr);



    let line;
    let posLine;
    let nomeLine;
    let dadoLine;

    for (let i = 0; i < dados.length; i++){
        line = document.createElement('tr');
        posLine = document.createElement('td');
        nomeLine = document.createElement('td');
        dadoLine = document.createElement('td');

        posLine.textContent = i+1;
        nomeLine.textContent = dados[i].usuario;
        dadoLine.textContent = dados[i].bestScore;

        line.append(posLine);
        line.append(nomeLine);
        line.append(dadoLine);

        table.append(line);
    }
}


updateTable();








const updateBestScore = async () => {
    try{
        const resposta = await fetch(`${serverRoute}/updateBestScore`, {
            method: 'POST',
            headers: {"Content-Type": "application/json"},
            body: JSON.stringify({
                mensagemEmail: {Subject: 'Novo recorde!', Content: `Você acabou de bater seu recorde no Pong do Paulo!\n\nSeu novo recorde é ${bestScore}`, Endereco: email},
                mensagemUsuario: {Nome: usuario, Senha: senha, Email: email, BestScore: bestScore}
            })
        });

        const dados = await resposta.json();

        console.log(dados.mensagem);
    } catch (error){
        console.error("Erro:", error);
    }
}







const canvas = document.getElementById('canvas');
const ctx = canvas.getContext('2d');


const width = canvas.width;
const height = canvas.height;

const tam = width / 20;

let fps = 120;
let gameSpeed = 1000 / fps;



let bestScore = 0;

let gameOver;
let p1;
let p2;
let ball;

const restartConfigs = () => {
    gameOver = false;


    p1 = {
        width: tam,
        height: tam*2,
        x: 0,
        y: height/2 - tam,
        color: 'red',
        speedY: 0,
        score: 0
    };


    p2 = {
        width: tam,
        height: tam*2,
        x: width-tam,
        y: height/2 - tam,
        color: 'blue',
        speedY: 0,
        score: 0
    };


    ball = {
        width: tam,
        height: tam,
        x: width/2 - tam,
        y: height/2 - tam,
        color: 'yellow',
        speedX: 2,
        speedY: 2
    };
};









document.addEventListener('keydown', (e) => {
    if (e.key == 'w'){
        p1.speedY = -1.2;
    } else if (e.key == 's'){
        p1.speedY = 1.2;
    };
});




document.addEventListener('keyup', (e) => {
    if (e.key === 'w' || e.key === 's'){
        p1.speedY = 0;
    };
});









const drawRect = (rectWidth, rectHeight, color, x, y) => {
    ctx.fillStyle = color;
    ctx.fillRect(x, y, rectWidth, rectHeight);
};





const drawAll = () => {
    ctx.clearRect(0, 0, width, height);

    drawRect(p1.width, p1.height, p1.color, p1.x, p1.y);
    drawRect(p2.width, p2.height, p2.color, p2.x, p2.y);
    drawRect(ball.width, ball.height, ball.color, ball.x, ball.y);


    ctx.font = '32px Arial';
    ctx.fillStyle = 'white';

    ctx.fillText(p1.score.toString(), p1.x, 50);
    ctx.fillText(p2.score.toString(), p2.x, 50);
};






const movementBall =() => {
    if (ball.x === 0){
        p2.score++;
        ball.speedX = -ball.speedX;
    } else if(ball.x === width - ball.width){
        p1.score++;

        ball.speedX = -ball.speedX;
    };



    if (ball.y === height - ball.height || ball.y === 0){
        ball.speedY = -ball.speedY;
    };



    ball.x += ball.speedX;
    ball.y += ball.speedY;
};





const movementPlayers = () => {
    p1.y += p1.speedY;
    

    const iaSpeed = 1.2;
    if (ball.y + ball.height < p2.y + p2.height/2){
        p2.y -= iaSpeed;
    } else if (ball.y + ball.height/2 > p2.y + p2.height/2){
        p2.y += iaSpeed;
    };



    if (p1.y < 0) p1.y = 0;
    if (p1.y + p1.height > height) p1.y = height - p1.height;

    if (p2.y < 0) p1.y = 0;
    if (p2.y + p2.height > height) p2.y = height - p2.height;
};





const updateCollision = () => {
    if (ball.x === p2.x - ball.width && (ball.y >= p2.y && ball.y+ball.height <= p2.y+p2.height)){
        ball.speedX = -ball.speedX;
    } else if (ball.x === p1.width && (ball.y >= p1.y && ball.y + ball.height <= p1.y + p1.height)){
        ball.speedX = -ball.speedX;
    };
};





const updateGameOver = () => {
    if (p2.score >= 3){
        if (p1.score > bestScore){
            bestScore = p1.score;
            console.log(bestScore);

            updateBestScore();
        };

        updateTable();
        gameOver = true;
    };
};






let timeOutID;

const gameLoop = () => {
    timeOutID = setTimeout(() => {
        updateGameOver();

        if (gameOver){
            clearTimeout(timeOutID);
            initGame();
            return;
        };


        movementPlayers();
        movementBall();
        updateCollision();
        drawAll();


        gameLoop();
    }, gameSpeed);
};





const initGame = () => {
    setTimeout(() => {
        restartConfigs();
        gameLoop();
    }, 3000);
}



restartConfigs();
drawAll();


initGame();