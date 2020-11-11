<?php
// WSDL del servicio
$servicio = 'http://localhost:65405/setUpUsers.svc?wsdl';
// Arreglo de parámetros
$parametros = array();
$parametros['user'] = "pruebas10";
$parametros['pass'] = "12345678a";
$parametros['oldUser'] = "pruebas1";
$parametros['newUser'] = "pruebas10";
$parametros['newPass'] = "12345678a";
// Se crea el cliente del servicio
$client = new soapclient($servicio);
// Se invoca el metodo que vamos a probar
/**
    El estilo de comunicación es "document" y tipo de uso "literal", los parametros deben ir en un arreglo
    Respuesta = objeto con atributos, contenido en otro objeto
 **/
try {
    //print_r($parametros);
    $result = $client->updateUser($parametros);

    //Para observar el Dump de lo que regresa, es puramente de debug
    echo 'Valor echo del servicio:<br>';
        echo $result->updateUserResult->code . '<br>';
        echo $result->updateUserResult->message . '<br>';
        echo $result->updateUserResult->status . '<br>';
    echo '<br>';
} catch (\Throwable $th) {
    echo "<center>". str_replace("host","server",$th->getMessage()) ."</center>";
}
//Validacion del producto a añadir
function validarJSONProd($JSON, $movimiento)
{
    $JSON = json_decode($JSON, true);

    if (json_last_error()) { //Json malformado
        return array(
            'code' => 305,
            'message' => getRespuesta(305),
            'data' => "N/A",
            'status' => 'fail'
        );
    } else { //Si faltan datos
        if (faltanDatos(array_keys($JSON), $movimiento)) {
            return array(
                'code' => 306,
                'message' => getRespuesta(306),
                'data' => "N/A",
                'status' => 'fail'
            );
        } else { //Verificar si no existe el ISBN
            switch ($movimiento) {
                case "set":
                    if (existeISBN($JSON["ISBN"])) { //Ya existe el dato
                        return array(
                            'code' => 303,
                            'message' => str_replace("*", $JSON["ISBN"], getRespuesta(303)),
                            'data' => "N/A",
                            'status' => 'fail'
                        );
                    } else { //No existe el dato
                        return "ok";
                    }

                case "update":
                    if (!existeISBN($JSON["ISBN"])) { //No existe el dato
                        return array(
                            'code' => 304,
                            'message' => str_replace("*", $JSON["ISBN"], getRespuesta(304)),
                            'data' => "N/A",
                            'status' => 'fail'
                        );
                    } else { //Si existe
                        return "ok";
                    }
            }
        }
    }
}
//Verifica ques esten completos los datos del json
function faltanDatos($keys, $movimiento)
{
    switch ($movimiento) {

        case "set":
            $keysArrays = ["Categoria", "ISBN", "Autor", "Nombre", "Editorial", "Año", "Costo"];
            $flag = false;

            foreach ($keysArrays as $key) {
                if (!in_array($key, $keys)) { //Si no existe el valor
                    $flag = true;
                    break;
                }
            }
            return $flag;
        case "update":
            $flag = false;

            foreach ($keys as $key) {
                if (!in_array("ISBN", $keys)) { //Si no existe el valor
                    $flag = true;
                    break;
                }
                if (!in_array("Categoria", $keys)) { //Si no existe el valor
                    $flag = true;
                    break;
                }
            }
            return $flag;
    }
}
?>
<html>

<head>
    <title>Formulario para actualizar usuarios</title>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.1.0/jquery.min.js"></script>
</head>
<style>
    .hide {
        display: none !important;
    }

    html,
    body {
        width: 100%;
        height: 100%;
        margin: 0;
        padding: 0;
        font-family: 'Open Sans', sans-serif;
        background-color: #3498db;
    }

    h1,
    h2,
    h3,
    h4,
    h5,
    h6 {
        font-weight: 200;
    }

    a {
        text-decoration: none;
    }

    p,
    li,
    a {
        font-size: 14px;
    }

    fieldset {
        margin: 0;
        padding: 0;
        border: none;
    }

    /* GRID */

    .twelve {
        width: 100%;
    }

    .eleven {
        width: 91.53%;
    }

    .ten {
        width: 83.06%;
    }

    .nine {
        width: 74.6%;
    }

    .eight {
        width: 66.13%;
    }

    .seven {
        width: 57.66%;
    }

    .six {
        width: 49.2%;
    }

    .five {
        width: 40.73%;
    }

    .four {
        width: 32.26%;
    }

    .three {
        width: 23.8%;
    }

    .two {
        width: 15.33%;
    }

    .one {
        width: 6.866%;
    }

    /* COLUMNS */

    .col {
        display: block;
        float: left;
        margin: 0 0 0 1.6%;
    }

    .col:first-of-type {
        margin-left: 0;
    }

    .container {
        width: 100%;
        max-width: 700px;
        margin: 0 auto;
        position: relative;
    }

    .row {
        padding: 20px 0;
    }

    /* CLEARFIX */

    .cf:before,
    .cf:after {
        content: " ";
        display: table;
    }

    .cf:after {
        clear: both;
    }

    .cf {
        *zoom: 1;
    }

    .wrapper {
        width: 100%;
        margin: 30px 0;
    }

    #titulo,
    #titulo h1 {
        list-style-type: none;
        background-color: #fff;
        text-align: center;
        margin: 0;
    }

    /* STEPS */

    .steps {
        list-style-type: none;
        margin: 0;
        padding: 0;
        background-color: #fff;
        text-align: center;
    }


    .steps li {
        display: inline-block;
        margin: 20px;
        color: #ccc;
        padding-bottom: 5px;
    }

    .steps li.is-active {
        border-bottom: 1px solid #3498db;
        color: #3498db;
    }

    /* FORM */

    .form-wrapper .section {
        padding: 0px 20px 30px 20px;
        -webkit-box-sizing: border-box;
        -moz-box-sizing: border-box;
        box-sizing: border-box;
        background-color: #fff;
        opacity: 0;
        -webkit-transform: scale(1, 0);
        -ms-transform: scale(1, 0);
        -o-transform: scale(1, 0);
        transform: scale(1, 0);
        -webkit-transform-origin: top center;
        -moz-transform-origin: top center;
        -ms-transform-origin: top center;
        -o-transform-origin: top center;
        transform-origin: top center;
        -webkit-transition: all 0.5s ease-in-out;
        -o-transition: all 0.5s ease-in-out;
        transition: all 0.5s ease-in-out;
        text-align: center;
        position: absolute;
        width: 100%;
        min-height: 300px
    }

    .form-wrapper .section h3 {
        margin-bottom: 30px;
    }

    .form-wrapper .section.is-active {
        opacity: 1;
        -webkit-transform: scale(1, 1);
        -ms-transform: scale(1, 1);
        -o-transform: scale(1, 1);
        transform: scale(1, 1);
    }

    .form-wrapper .button,
    .form-wrapper .submit {
        background-color: #3498db;
        display: inline-block;
        padding: 8px 30px;
        color: #fff;
        cursor: pointer;
        font-size: 14px !important;
        font-family: 'Open Sans', sans-serif !important;
        position: absolute;
        right: 20px;
        bottom: 20px;
    }

    .form-wrapper .button-disabled {
        background-color: grey;
    }

    .form-wrapper .submit {
        border: none;
        outline: none;
        -webkit-box-sizing: content-box;
        -moz-box-sizing: content-box;
        box-sizing: content-box;
        -webkit-appearance: none;
        -moz-appearance: none;
        appearance: none;
    }

    .form-wrapper input[type="text"],
    .form-wrapper input[type="password"] {
        display: block;
        padding: 10px;
        margin: 10px auto;
        background-color: #f1f1f1;
        border: none;
        width: 50%;
        outline: none;
        font-size: 14px !important;
        font-family: 'Open Sans', sans-serif !important;
    }

    .form-wrapper input[type="radio"] {
        display: none;
    }

    .form-wrapper input[type="radio"]+label {
        display: block;
        border: 1px solid #ccc;
        width: 100%;
        max-width: 100%;
        padding: 10px;
        -webkit-box-sizing: border-box;
        -moz-box-sizing: border-box;
        box-sizing: border-box;
        cursor: pointer;
        position: relative;
    }

    .form-wrapper input[type="radio"]+label:before {
        content: "✔";
        position: absolute;
        right: -10px;
        top: -10px;
        width: 30px;
        height: 30px;
        line-height: 30px;
        border-radius: 100%;
        background-color: #3498db;
        color: #fff;
        display: none;
    }

    .form-wrapper input[type="radio"]:checked+label:before {
        display: block;
    }

    .form-wrapper input[type="radio"]+label h4 {
        margin: 15px;
        color: #ccc;
    }

    .form-wrapper input[type="radio"]:checked+label {
        border: 1px solid #3498db;
    }

    .form-wrapper input[type="radio"]:checked+label h4 {
        color: #3498db;
    }
</style>

<body>
    <div class="container">
        <div class="wrapper">
            <div id="Titulo">
                <h1>Gestión de usuarios</h1>
            </div>
            <ul class="steps">
                <li class="is-active">Tipo de operacón</li>
                <li>Insertar datos</li>
                <li>Respuesta</li>
            </ul>
            <form class="form-wrapper">
                <fieldset class="section is-active">
                    <script id="ajax">
                        //AJAX
                        function getObjXMLHttpRequest() {
                            var http;
                            if (window.ActiveXObject) http = new ActiveXObject("Msxml2.XMLHttp");
                            else http = new XMLHttpRequest();
                            return http;
                        }

                        function seleccionarOperacion(ope) {
                            var xhttp = getObjXMLHttpRequest();
                            xhttp.onreadystatechange = function() {
                                if (this.readyState == 4 && this.status == 200) {
                                    switch (ope) {
                                        case "updateUser":
                                            document.getElementById("searchedUser").classList.add("hide");
                                            document.getElementById("userInfoJSON").classList.add("hide");

                                            document.getElementById("newUser").classList.remove("hide");
                                            document.getElementById("newPass").classList.remove("hide");
                                            break;

                                        case "setUser":
                                            document.getElementById("searchedUser").classList.add("hide");
                                            document.getElementById("newUser").classList.add("hide");
                                            document.getElementById("newPass").classList.add("hide");

                                            document.getElementById("userInfoJSON").classList.remove("hide");
                                            break;

                                        default:
                                            document.getElementById("newUser").classList.add("hide");
                                            document.getElementById("newPass").classList.add("hide");

                                            document.getElementById("searchedUser").classList.remove("hide");
                                            document.getElementById("userInfoJSON").classList.remove("hide");
                                            break;
                                    }
                                    document.getElementById("btn_ope").classList.remove("button-disabled");
                                    document.getElementById("btn_ope").removeAttribute("disabled");
                                }
                            }
                            xhttp.open("GET", "", true);
                            xhttp.send(null);
                        }
                    </script>
                    <h3>Operación</h3>
                    <div class="row cf">
                        <div class="four col">
                            <input type="radio" name="operacion" id="r1">
                            <label for="r1" onClick='seleccionarOperacion("setUser")'>
                                <h4>Insertar usuario</h4>
                            </label>
                        </div>
                        <div class="four col">
                            <input type="radio" name="operacion" id="r2">
                            <label for="r2" onClick='seleccionarOperacion("updateUser")'>
                                <h4>Actualizar usuario</h4>
                            </label>
                        </div>
                        <div class="four col">
                            <input type="radio" name="operacion" id="r3">
                            <label for="r3" onClick='seleccionarOperacion("setUserInfo")'>
                                <h4>Insertar datos de usuario</h4>
                            </label>
                        </div>
                        <div class="four col">
                            <input type="radio" name="operacion" id="r4">
                            <label for="r4" onClick='seleccionarOperacion("updateUserInfo")'>
                                <h4>Actualizar datos de usuario</h4>
                            </label>
                        </div>
                    </div>
                    <button id="btn_ope" class="button button-disabled" disabled>Next</button>
                </fieldset>
                <fieldset class="section">
                    <h3>Datos a insertar</h3>
                    <div id="inputs">
                        <input type="text" name="user" id="user" placeholder="Usuario" onchange="validateSection(this.parent)">
                        <input type="text" name="pass" id="pass" placeholder="Contraseña" onchange="validateSection(this.parent)">
                        <input type="text" name="newUser" id="newUser" placeholder="Usuario nuevo" onchange="validateSection(this.parent)">
                        <input type="text" name="newPass" id="newPass" placeholder="Contraseña nueva" onchange="validateSection(this.parent)">
                        <input type="text" name="searchedUser" id="searchedUser" placeholder="Usuario buscado" onchange="validateSection(this.parent)">
                        <input type="text" name="userInfoJSON" id="userInfoJSON" placeholder="Información JSON del usuario" onchange="validateSection(this.parent)">
                    </div>
                    <button class="button button-disabled" onclick="enviarDatos()" disabled>Next</button>
                    <script>
                        function validateSection(nodoPadre){
                            let inputs = nodoPadre.querySelectorAll("input")
                            let flag = true;
                            let btn = nodoPadre.nextElementSibling
                            inputs.forEach(input => {
                                if (input.value == "" && !input.classList.contains("hide")) {
                                    btn.classList.add("button-disabled");
                                    btn.setAttribute("disabled","");
                                    flag=false;
                                    return;
                                }
                            });
                            if(flag){
                                btn.classList.remove("button-disabled");
                                btn.removeAttribute("disabled");
                            }
                        }
                        function enviarDatos(){
                            var xhttp = getXMLHttpRequest();
                            xhttp.onreadystatechange = function(){
                                if (this.readyState == 4 && this.status == 200){
                                    //Dependiendo la operacion seleccionada
                                    switch()
                                }
                            };

                            xhttp.open("POST", "consultarBD.php", true);
                            xhttp.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                            xhttp.send("nombre="+document.forms[0]["nombre"].value);
                        }
                    </script>
                </fieldset>
                <fieldset class="section">
                    <h3>Esta es la respuesta del servidor</h3>
                    <p>Los datos fueron...</p>
                    <button class="button">Nueva operación</button>
                </fieldset>
            </form>
        </div>
    </div>
    <script>
        $(document).ready(function() {
            $(".form-wrapper .button").click(function() {
                var button = $(this);
                var currentSection = button.parents(".section");
                var currentSectionIndex = currentSection.index();
                var headerSection = $('.steps li').eq(currentSectionIndex);
                currentSection.removeClass("is-active").next().addClass("is-active");
                headerSection.removeClass("is-active").next().addClass("is-active");

                $(".form-wrapper").submit(function(e) {
                    e.preventDefault();
                });

                if (currentSectionIndex === 2) {
                    $(document).find(".form-wrapper .section").first().addClass("is-active");
                    $(document).find(".steps li").first().addClass("is-active");
                }
            });
        });
    </script>
</body>

</html>