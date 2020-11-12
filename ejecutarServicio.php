<?php
if(isset($_POST["operacion"])){
    // WSDL del servicio
    $servicio = 'http://localhost:65405/setUpUsers.svc?wsdl';
    // Se crea el cliente del servicio
    $client = new soapclient($servicio);
    // Se invoca el metodo que vamos a probar
    /**
        El estilo de comunicación es "document" y tipo de uso "literal", los parametros deben ir en un arreglo
        Respuesta = objeto con atributos, contenido en otro objeto
    **/
    // Arreglo de parámetros
    $parametros = array();

    switch($_POST["operacion"]){
        case "updateUser":
            $parametros['user'] = $_POST["user"];
            $parametros['pass'] = $_POST["pass"];
            $parametros['oldUser'] = $_POST["oldUser"];
            $parametros['newUser'] = $_POST["newUser"];
            $parametros['newPass'] = $_POST["newPass"];
            try {
                $result = $client->updateUser($parametros);
                $return = array(
                    "code" => $result->updateUserResult->code,
                    "message" => $result->updateUserResult->message,
                    "status" => $result->updateUserResult->status
                );
                echo json_encode($return);
            } catch (\Throwable $th) {
                echo json_encode( array("error" => "<center>". str_replace("host","server",$th->getMessage()) ."</center>") );
            }
            break;
        case "setUserInfo":
            $parametros['user'] = $_POST["user"];
            $parametros['pass'] = $_POST["pass"];
            $parametros['searchedUser'] = $_POST["searchedUser"];
            $parametros['userInfoJSON'] = $_POST["userInfoJSON"];
            try {
                $result = $client->setUserInfo($parametros);
                $return = array(
                    "code" => $result->setUserInfoResult->code,
                    "message" => $result->setUserInfoResult->message,
                    "data" => $result->setUserInfoResult->data,
                    "status" => $result->setUserInfoResult->status
                );
                echo json_encode($return);
            } catch (\Throwable $th) {
                echo json_encode( array("error" => "<center>". str_replace("host","server",$th->getMessage()) ."</center>") );
            }
            break;
        case "setUser":
            $parametros['user'] = $_POST["user"];
            $parametros['pass'] = $_POST["pass"];
            $parametros['searchedUser'] = $_POST["searchedUser"];
            $parametros['userInfoJSON'] = $_POST["userInfoJSON"];
            try {
                $result = $client->setUser($parametros);
                $return = array(
                    "code" => $result->setUserResult->code,
                    "message" => $result->setUserResult->message,
                    "data" => $result->setUserResult->data,
                    "status" => $result->setUserResult->status
                );
                echo json_encode($return);
            } catch (\Throwable $th) {
                echo json_encode( array("error" => "<center>". str_replace("host","server",$th->getMessage()) ."</center>") );
            }
            break;
        case "updateUserInfo":
            $parametros['user'] = $_POST["user"];
            $parametros['pass'] = $_POST["pass"];
            $parametros['searchedUser'] = $_POST["searchedUser"];
            $parametros['userInfoJSON'] = $_POST["userInfoJSON"];
            try {
                $result = $client->updateUserInfo($parametros);
                $return = array(
                    "code" => $result->updateUserInfoResult->code,
                    "message" => $result->updateUserInfoResult->message,
                    "data" => $result->updateUserInfoResult->data,
                    "status" => $result->updateUserInfoResult->status
                );
                echo json_encode($return);
            } catch (\Throwable $th) {
                echo json_encode( array("error" => "<center>". str_replace("host","server",$th->getMessage()) ."</center>") );
            }
            break;
    }
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