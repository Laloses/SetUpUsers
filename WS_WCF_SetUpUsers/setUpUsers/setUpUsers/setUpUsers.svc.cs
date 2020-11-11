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
            AuthSecret = "3nnFWYwLeCe1LGAQprH0kqjm2SnByLWxYMYhoZDP",
            BasePath = "https://prueba-8e491.firebaseio.com/"
        };

        IFirebaseClient firebaseClient;

        public Respuesta updateUser(string user, string pass, string oldUser, string newUser, string newPass)
        {
            firebaseClient = new FireSharp.FirebaseClient(config);
            Respuesta resp = new Respuesta();

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
                dynamic resAccessUser = JsonConvert.DeserializeObject( firebaseClient.Get("usuarios_info/" + user.ToString()).Body );
                string rol = resAccessUser["rol"];
                if(rol != "RH")
                    return new Respuesta() { code = "502", message = getRespuesta(502), status = "Deber ser de recursos humanos para realizar esta acción."};
                else
                {
                    //Si no cumple con los requisitos de la nueva contraseña
                    if (newPass.Length < 8 || !ValidatePassword(newPass) )
                        return new Respuesta() { code = "302", message = getRespuesta(302), status = "La nueva contraseña no es válida." };
                    else
                    {

                        //Verificar que existe el usuario a modificar
                        dynamic existOldUser = JsonConvert.DeserializeObject( firebaseClient.Get("usuarios/" + oldUser.ToString()).Body );
                        if(existOldUser == "" || existOldUser == null)
                            return new Respuesta() { code = "503", message = getRespuesta(503), status = "Verifica que exista el usuario a modificar." };
                        else
                        {
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
                    }
                }
            }

            return new Respuesta() { code = "999", message = getRespuesta(999), status = "Llegó al final del proceso sin haber realizado una acción." };
        }

        public Respuesta setUser(string user, string pass, string searchedUser, string userInfoJSON)
        {
            firebaseClient = new FireSharp.FirebaseClient(config);
            Respuesta resp = new Respuesta();


            resp.code = "10";
            return resp;
        }

        public Respuesta setUserInfo(string user, string pass, string searchedUser, string userInfoJSON)
        {
            firebaseClient = new FireSharp.FirebaseClient(config);
            Respuesta resp = new Respuesta();


            resp.code = "10";
            return resp;
        }

        public Respuesta updateUserInfo(string user, string pass, string searchedUser, string userInfoJSON)
        {
            firebaseClient = new FireSharp.FirebaseClient(config);
            Respuesta resp = new Respuesta();


            resp.code = "10";
            return resp;
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
    }
}
