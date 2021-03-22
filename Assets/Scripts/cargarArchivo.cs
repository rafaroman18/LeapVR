using System;
using System.Collections;
using System.Net;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using Leap.Unity.Interaction;
using UnityGLTF;


public class cargarArchivo : MonoBehaviour
{
    public GLTFComponent gltfLoader;
    public PlaceObjectOnGrid textBox;
    public string objeto;

    void Start()
    {
        
    }

    public void insertarObjetoEscena(string name)
    {

        //La creamos por si no existe de primeras
        Directory.CreateDirectory("ZIP");

        eliminaCarpeta();

        objeto = name;

        //1 - Se buscan objetos con el correspondiente tag y se obtiene el JSON
        //2 - Sacamos el UID del primero 
        string UID = buscarObjetosJSON(name);

        //Si se ha encontrado el objeto, entonces continuamos con la ejecución
        if (UID != "null")
        {
            //3 - Creamos un GET con authorization y obtenemos otro JSON con enlaces temporales
            //4 - El primer enlace será el que necesitamos
            string link = obtenerEnlaceObjeto(UID);

            //5 - GET al enlace y obtenemos un .zip
            //6 - Descomprimimos y el archivo .gtlf será creado
            creaObjeto(link);
        }
        else
        {
            textBox.CambiarTexto("No se ha encontrado el objeto especificado");

            StartCoroutine(cerrarTextBox());
        }
    }


    //Busca el objeto y devuelve el UID del primero que queremos crear
    private string buscarObjetosJSON(string name)
    {
        string uri = "https://api.sketchfab.com/v3/search?type=models&q=" + name + "&downloadable=true&pbr_type=false";
        string json;
        string result = "null";

        try
        {
            //Obtenemos el JSON
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Accept = "application/json";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())

            using (StreamReader reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }

            //Leemos el texto json obtenido, lo dividimos por lineas
            //y buscamos el primer UID que contenga
            bool encontrado = false;
            string[] jsonDividido = json.Split(',');
            string UID = "";

            for (int i = 0; i < jsonDividido.Length && !encontrado; ++i)
            {

                if (jsonDividido[i].Contains("uid"))
                {
                    encontrado = true;
                    int j = 0;
                    while (jsonDividido[i][j] != 'd')
                    {
                        j++;
                    }

                    j += 4;

                    while (jsonDividido[i][j] != '"')
                    {
                        UID += jsonDividido[i][j];
                        j++;
                    }

                }

            }

            if (encontrado)
                result = UID;

        }

        catch (Exception e) { textBox.CambiarTexto("Límite de peticiones alcanzado"); StartCoroutine(cerrarTextBox()); }

        return result;

    }


    //Realiza un GET con el UID dado y devuelve el enlace temporal
    private string obtenerEnlaceObjeto(string UID)
    {
        string uri = "https://api.sketchfab.com/v3/models/" + UID + "/download";
        string json;

        //Obtenemos el JSON
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
        request.Accept = "application/json";
        request.Headers["Authorization"] = "INTRODUCE TOKEN API OF USER";

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            json = reader.ReadToEnd();
        }

        //Leemos el texto json obtenido, lo dividimos por lineas
        //y buscamos el primer enlace que contenga
        //ya que es el que corresponde con el tipo de objeto gltf
        bool encontrado = false;
        string[] jsonDividido = json.Split(',');
        string link = "";

        for (int i = 0; i < jsonDividido.Length && !encontrado; ++i)
        {

            if (jsonDividido[i].Contains("gltf"))
            {
                encontrado = true;
                int j = 0;

                while (jsonDividido[i][j] != 'h')
                {
                    j++;
                }

                while (jsonDividido[i][j] != '"')
                {

                    //Arreglamos las barras del enlace
                    if (jsonDividido[i][j] != '\\')
                    {
                        link += jsonDividido[i][j];
                    }
                    j++;
                }

            }

        }

        return link;
    }


    //Realiza un GET al enlace temporal, descarga el .zip
    //y crea el objeto en la escena
    private async void creaObjeto(string link)
    {

        //Descargamos el .zip
        using (WebClient client = new WebClient())
        {
            client.DownloadFile(new Uri(link), "ZIP/objeto.zip");
        }


        //Descomprimimos el .zip
        ZipFile.ExtractToDirectory("ZIP/objeto.zip", "ZIP");


        //Creamos el objeto en la escena
        gltfLoader.GLTFUri = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ZIP/scene.gltf");
        textBox.CambiarTexto("Cargando");
        try
        {
            await gltfLoader.Load();
            GameObject objectCreated = transform.Find("OSG_Scene").gameObject;
            objectCreated.name = objeto;
            objectCreated.tag = "objetoCreado";

            //Desactivamos los Box Collider para evitar colisiones con objetos grandes
            MeshCollider[] colls = objectCreated.GetComponentsInChildren<MeshCollider>();
            foreach (MeshCollider meshC in colls)
            {
                meshC.enabled = false;
            }



            //Colocamos el tamaño correcto del objeto
            await textBox.IniciaEscala(objectCreated);

            //Una vez seleccionado la escala, se crean sus componentes 
            //para que el objeto sea interactivo
            foreach (MeshCollider meshC in colls)
            {
                meshC.enabled = true;
            }
            objectCreated.AddComponent<Rigidbody>();
            objectCreated.AddComponent<InteractionBehaviour>();
            objectCreated.GetComponent<Rigidbody>().useGravity = true;
            objectCreated.GetComponent<InteractionBehaviour>().manager = FindObjectOfType<InteractionManager>();

            //Se indica el fin de la ejecucion
            textBox.CambiarTexto("Listo");

        }
        catch (Exception e) { textBox.CambiarTexto("Error al cargar el objeto"); gltfLoader.StopAllCoroutines(); }
        finally { StartCoroutine(cerrarTextBox()); }
    }


    private void eliminaCarpeta()
    {

        //Eliminamos la carpeta de ZIP y su contenido
        if (Directory.Exists("ZIP"))
            Directory.Delete("ZIP", true);

        //La volvemos a crear
        Directory.CreateDirectory("ZIP");
    }

    //Al cerrar la aplicación eliminaremos la carpeta ZIP
    void OnDestroy()
    {
         textBox.DesactivarTexto();
        eliminaCarpeta();
    }

    private IEnumerator cerrarTextBox()
    {
        yield return new WaitForSeconds(4f);
        textBox.DesactivarTexto();
    }

}




