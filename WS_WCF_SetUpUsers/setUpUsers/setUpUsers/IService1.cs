using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using FireSharp.Config;
using FireSharp.Interfaces;

namespace setUpUsers
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "IService1" en el código y en el archivo de configuración a la vez.
    [ServiceContract]
    public interface IServiceSetUpUsuarios
    {
        // TODO: agregue aquí sus operaciones de servicio
        [OperationContract]
        Respuesta updateUser(string user, string pass, string oldUser, string newUser, string newPass);
        [OperationContract]
        Respuesta setUserInfo(string user, string pass, string searchedUser, string userInfoJSON);
        [OperationContract]
        Respuesta updateUserInfo(string user, string pass, string searchedUser, string userInfoJSON);
        [OperationContract]
        Respuesta setUser(string user, string pass, string searchedUser, string userInfoJSON);

    }

    // Utilice un contrato de datos, como se ilustra en el ejemplo siguiente, para agregar tipos compuestos a las operaciones de servicio.
    [DataContract]
    public class Respuesta
    {
        [DataMember] public string code { get; set; }
        [DataMember] public string message { get; set; }
        [DataMember] public string data { get; set; }
        [DataMember] public string status { get; set; }
    }
    
}
