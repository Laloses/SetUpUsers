using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Echovoice.JSON;

using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace setUpUsers
{
    // NOTE: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione Service1.svc o Service1.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class ServiceSetUpUsuarios : IServiceSetUpUsuarios
    {
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "",
            BasePath = ""
        };

        IFirebaseClient firebaseClient;

        public Respuesta updateUser(string user, string pass, string oldUser, string newUser, string newPass)
        {
            firebaseClient = new FireSharp.FirebaseClient(config);

            string resVal = validarCredenciales(user, pass); //Validacion del user - pass
            
            if (resVal != "ok")
            {
                if(resVal=="user")
                    return new Respuesta() { code = "500", message = getRespuesta(500), status = "El nombre de usuario de tu cuenta es incorrecto." };
                if(resVal=="pass")
                    return new Respuesta() { code = "302", message = getRespuesta(302), status = "La contraseña de tu cuenta es incorrecta." };
            }
            else
            {
                //Validacion del usuario de acceso (RH)
                dynamic resAccessUser = JsonConvert.DeserializeObject( firebaseClient.Get("usuarios_info/" + user).Body );
                string rol = resAccessUser["rol"];
                if(rol != "RH")
                    return new Respuesta() { code = "502", message = getRespuesta(502), status = "Deber ser de recursos humanos para realizar esta acción."};

                //Si no cumple con los requisitos de la nueva contraseña
                if (newPass.Length < 8 || !ValidatePassword(newPass) )
                    return new Respuesta() { code = "302", message = getRespuesta(302), status = "La nueva contraseña no es válida." };

                //Verificar que el nuevo usuario no tenga espacios, lleve numeros y letras
                if (!ValidatePassword(newUser))
                    return new Respuesta() { code = "307", message = getRespuesta(307), status = "El nuevo usuario no es válido." };

                //Verificar que existe el usuario a modificar
                dynamic existOldUser = JsonConvert.DeserializeObject( firebaseClient.Get("usuarios/" + oldUser.ToString()).Body );
                if(existOldUser == "" || existOldUser == null)
                return new Respuesta() { code = "503", message = getRespuesta(503), status = "Verifica que exista el usuario a modificar." };

                //Encriptar nueva contraseña
                MD5 md5 = MD5CryptoServiceProvider.Create();
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] stream = null;
                StringBuilder sb = new StringBuilder();
                stream = md5.ComputeHash(encoding.GetBytes(newPass));
                for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
                newPass = sb.ToString();

                //Actualizar nombre de usuario y contraseña del documento /usuarios.json
                firebaseClient.Delete("usuarios/" + oldUser);
                firebaseClient.Set("usuarios/"+newUser, newPass);

                //Si se inserto correctamente
                return new Respuesta() { code = "400", message = getRespuesta(400), status = "Actualizacion de credenciales completa." };
            }

            return new Respuesta() { code = "999", message = getRespuesta(999), status = "Llegó al final del proceso sin haber realizado una acción." };
        }

        public Respuesta setUserInfo(string user, string pass, string searchedUser, string userInfoJSON)
        {
            firebaseClient = new FireSharp.FirebaseClient(config);

            string resVal = validarCredenciales(user, pass); //Validacion del user - pass

            if (resVal != "ok")
            {
                if (resVal == "user")
                    return new Respuesta() { code = "500", message = getRespuesta(500), data = "", status = "El nombre de usuario de tu cuenta es incorrecto." };
                if (resVal == "pass")
                    return new Respuesta() { code = "302", message = getRespuesta(302), data = "", status = "La contraseña de tu cuenta es incorrecta." };
            }
            else
            {
                //Validacion del usuario de acceso (RH)
                dynamic resAccessUser = JsonConvert.DeserializeObject(firebaseClient.Get("usuarios_info/" + user).Body);
                string rol = resAccessUser["rol"];
                if (rol != "RH")
                    return new Respuesta() { code = "502", message = getRespuesta(502), data = "", status = "Deber ser de recursos humanos para realizar esta acción." };

                //No existencia del usuario en el documento /usuarios
                if (!existeUser(searchedUser))
                    return new Respuesta() { code = "503", message = getRespuesta(503), data = "", status = "No existe el usuario "+searchedUser+" en el documento /usuarios" };

                //Existencia previa de los datos del Usuario buscado
                if (existeUserInfo(searchedUser))
                    return new Respuesta() { code = "504", message = getRespuesta(504), data = "", status = "Ya existen datos del usuario \"" + searchedUser + "\" en el documento /usuarios_info" };

                //Sintaxis del JSON.
                try
                {
                    dynamic json = JsonConvert.DeserializeObject(userInfoJSON);

                    //Completitud del JSON.
                    if (new[]{ "correo", "nombre", "rol", "telefono" }.Any(parametro => json[parametro] == null))
                        return new Respuesta() { code = "306", message = getRespuesta(306), data = "", status = "Revisa los datos faltantes en el json." };

                    //Insertar datos en documento usuarios_info
                    var data = new
                    {
                        correo = (string)json["correo"],
                        nombre = (string)json["nombre"],
                        rol = (string)json["rol"],
                        telefono = (string)json["telefono"]
                    };
                    firebaseClient.Set("usuarios_info/"+searchedUser, data);

                    //Si se inserto correctamente
                    return new Respuesta() { code = "401", message = getRespuesta(401), data = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), status = "Inserción de datos completa." };

                } catch(Exception error)
                {
                    return new Respuesta() { code = "305", message = getRespuesta(305), data = "", status = "Verifica la sintaxis del json que estás ingresando." };
                }


            }

            return new Respuesta() { code = "999", message = getRespuesta(999), status = "Llegó al final del proceso sin haber realizado alguna acción." };
        }

        public Respuesta setUser(string user, string pass, string searchedUser, string userInfoJSON)
        {
            firebaseClient = new FireSharp.FirebaseClient(config);

            string resVal = validarCredenciales(user, pass); //Validacion del user - pass

            if (resVal != "ok")
            {
                if (resVal == "user")
                    return new Respuesta() { code = "500", message = getRespuesta(500), data = "", status = "El nombre de usuario de tu cuenta es incorrecto." };
                if (resVal == "pass")
                    return new Respuesta() { code = "302", message = getRespuesta(302), data = "", status = "La contraseña de tu cuenta es incorrecta." };
            }
            else
            {
                //Validacion del usuario de acceso (RH)
                dynamic resAccessUser = JsonConvert.DeserializeObject(firebaseClient.Get("usuarios_info/" + user).Body);
                string rol = resAccessUser["rol"];
                if (rol != "RH")
                    return new Respuesta() { code = "502", message = getRespuesta(502), data = "", status = "Deber ser de recursos humanos para realizar esta acción." };

                //existencia del usuario en el documento /usuarios
                if (existeUser(searchedUser))
                    return new Respuesta() { code = "504", message = getRespuesta(504), data = "", status = "Ya existe el usuario " + searchedUser + " en el documento /usuarios" };

                //Sintaxis del JSON.
                try
                {
                    dynamic json = JsonConvert.DeserializeObject(userInfoJSON);

                    //Completitud del JSON.
                    if ( json[searchedUser] == null)
                        return new Respuesta() { code = "306", message = getRespuesta(306), data = "", status = "Revisa los datos faltantes en el json." };

                    //Encriptar nueva contraseña
                    MD5 md5 = MD5CryptoServiceProvider.Create();
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] stream = null;
                    StringBuilder sb = new StringBuilder();
                    stream = md5.ComputeHash(encoding.GetBytes((string)json[searchedUser]));
                    for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
                    string password = sb.ToString();

                    //Insertar datos en documento usuarios_info
                    firebaseClient.Set("usuarios/" + searchedUser, password);

                    //Si se inserto correctamente
                    return new Respuesta() { code = "402", message = getRespuesta(402), data = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), status = "Inserción de datos completa." };

                }
                catch (Exception error)
                {
                    return new Respuesta() { code = "305", message = getRespuesta(305), data = "", status = "Verifica la sintaxis del json que estás ingresando." };
                }


            }

            return new Respuesta() { code = "999", message = getRespuesta(999), status = "Llegó al final del proceso sin haber realizado alguna acción." };
        }

        public Respuesta updateUserInfo(string user, string pass, string searchedUser, string userInfoJSON)
        {
            firebaseClient = new FireSharp.FirebaseClient(config);

            string resVal = validarCredenciales(user, pass); //Validacion del user - pass

            if (resVal != "ok")
            {
                if (resVal == "user")
                    return new Respuesta() { code = "500", message = getRespuesta(500), data = "", status = "El nombre de usuario de tu cuenta es incorrecto." };
                if (resVal == "pass")
                    return new Respuesta() { code = "302", message = getRespuesta(302), data = "", status = "La contraseña de tu cuenta es incorrecta." };
            }
            else
            {
                //Validacion del usuario de acceso (RH)
                dynamic resAccessUser = JsonConvert.DeserializeObject( firebaseClient.Get("usuarios_info/" + user).Body );
                string rol = resAccessUser["rol"];
                if (rol != "RH")
                    return new Respuesta() { code = "502", message = getRespuesta(502), data = "", status = "Deber ser de recursos humanos para realizar esta acción." };

                //No existencia del usuario en el documento /usuarios
                if (!existeUser(searchedUser))
                    return new Respuesta() { code = "503", message = getRespuesta(503), data = "", status = "No existe el usuario " + searchedUser + " en el documento /usuarios" };

                //Existencia previa de los datos en el documento /usuarios_info
                if (!existeUserInfo(searchedUser))
                    return new Respuesta() { code = "503", message = getRespuesta(503), data = "", status = "No existen datos del usuario \"" + searchedUser + "\" en el documento /usuarios_info" };

                //Sintaxis del JSON.
                try
                {
                    dynamic json = JsonConvert.DeserializeObject(userInfoJSON);

                    //Completitud del JSON.
                    if (new[] { "correo", "nombre", "rol", "telefono" }.Any(parametro => json[parametro] == null))
                        return new Respuesta() { code = "306", message = getRespuesta(306), data = "", status = "Revisa los datos faltantes en el json." };
                    

                    //Insertar datos en documento usuarios_info
                    var data = new
                    {
                        correo = (string)json["correo"],
                        nombre = (string)json["nombre"],
                        rol = (string)json["rol"],
                        telefono = (string)json["telefono"]
                    };
                    firebaseClient.Set("usuarios/" + searchedUser, data);

                    //Si se inserto correctamente
                    return new Respuesta() { code = "403", message = getRespuesta(403), data = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), status = "Inserción de datos completa." };

                }
                catch (Exception error)
                {
                    return new Respuesta() { code = "305", message = getRespuesta(305), data = "", status = "Verifica la sintaxis del json que estás ingresando." };
                }


            }

            return new Respuesta() { code = "999", message = getRespuesta(999), status = "Llegó al final del proceso sin haber realizado alguna acción." };
        }
        
        private string validarCredenciales(string usuario, string contraseña)
        {

            firebaseClient = new FireSharp.FirebaseClient(config);
            FirebaseResponse response =  firebaseClient.Get("usuarios");
            dynamic json = JsonConvert.DeserializeObject(response.Body);
            
            if (json[usuario] == null)
                return "user";
            else
            {
                //Desencriptar contraseña
                MD5 md5 = MD5CryptoServiceProvider.Create();
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] stream = null;
                StringBuilder sb = new StringBuilder();
                stream = md5.ComputeHash(encoding.GetBytes(contraseña));
                for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);

                contraseña = sb.ToString();

                if (json[usuario] != contraseña)
                    return "pass";
                else
                    return "ok";
            }
        }

        private static bool ValidatePassword(string passWord)
        {
            int validConditions = 0;
            foreach (char c in passWord)
            {
                if (c == ' ')
                {
                    return false;
                }
            }
            foreach (char c in passWord)
            {
                if (c >= 'a' && c <= 'z')
                {
                    validConditions++;
                    break;
                }
            }

            foreach (char c in passWord)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    validConditions++;
                    break;
                }
            }
            if (validConditions == 0) return false;
            foreach (char c in passWord)
            {
                if (c >= '0' && c <= '9')
                {
                    validConditions++;
                    break;
                }
            }
            if (validConditions == 1) return false;
            /*
            if (validConditions == 2)
            {
                char[] special = { '@', '#', '$', '%', '^', '&', '+', '=' }; // or whatever
                if (passWord.IndexOfAny(special) == -1) return false;
            }
            */
            return true;

        }

        private string getRespuesta(int code)
        {
            firebaseClient = new FireSharp.FirebaseClient(config);
            return firebaseClient.Get("respuestas/" + code.ToString()).Body.Replace("\"","");
        }

        private bool existeUser(string user)
        {
            firebaseClient = new FireSharp.FirebaseClient(config);
            dynamic existeUser = JsonConvert.DeserializeObject(firebaseClient.Get("usuarios/" + user).Body);

            if (existeUser == null)
                return false;
            else
                return true;
        }

        private bool existeUserInfo(string user)
        {
            firebaseClient = new FireSharp.FirebaseClient(config);
            dynamic existeUser = JsonConvert.DeserializeObject(firebaseClient.Get("usuarios_info/" + user).Body);

            if (existeUser == null)
                return false;
            else
                return true;
        }
    }
}
