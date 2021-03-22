using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


public class Receive : MonoBehaviour
{
    public static int ID = 1;

    //URL de la cola en la nube
    private static string url;

    //Creamos las variables de conexion requerida
    private static ConnectionFactory factory;
    private static IConnection connection;
    private static IModel channel;
    private static EventingBasicConsumer consumer;

    public GameObject azul;
    public GameObject roja;
    public GameObject amarilla;
    public GameObject morada;
    public GameObject verde;
    public GameObject naranja;
    public GameObject Nave;
    public GameObject Warehouse;
    public GameObject Fabric;


    private string pieza_recibida;

    void Awake()
    {

        pieza_recibida = "ninguna";

        url = "URL FROM CLOUD AMQP";
        factory = new ConnectionFactory();

        //Conectamos con la direccion escrita
        try
        {
            factory.Uri = new Uri(url);
        }
        catch (Exception e) { Debug.Log("Error al conectar con CloudRabbitMQ"); throw (e); }

        Debug.Log("Conexión con RabbitMQ establecida");

        //Creamos un canal de comunicacion
        connection = factory.CreateConnection();

        //Creamos una forma de consumir de la cola
        channel = connection.CreateModel();

        //Creamos un consumidor
        consumer = new EventingBasicConsumer(channel);

        //Nos mantenemos consumiendo hasta que la aplicación pare
        consumer.Received += (model, ea) =>
                                {
                                    var body = ea.Body;
                                    Debug.Log("[x] Mensaje recibido correctamente: " + Encoding.UTF8.GetString(body));
                                    pieza_recibida = Encoding.UTF8.GetString(body);
                                };

        //Comenzamos a consumir
        channel.BasicConsume(queue: "piezas",
            autoAck: true,
            consumer: consumer);

    }

    void Update()
    {
        //Si el contenido de la variable no se ha modificado, no 
        //haremos otra cosa
        if (pieza_recibida != "ninguna")
        {
            //Cambiamos el entorno
            if (pieza_recibida == "entorno")
            {
                CambiarEntorno();
            }
            else
            {
                if (pieza_recibida == "eliminar")
                {
                    EliminarTablero();
                }
                else
                {
                    if (pieza_recibida.Contains("tictactoe"))

                        TicTacToe();

                    else
                    {
                        if (pieza_recibida == "guiada")

                            Guiada();

                        else
                        {
                            if (pieza_recibida == "libre")
                            {
                                Libre();
                            }
                            else
                            {
                                //Segun si se quiere destruir o crear, hará una cosa u otra
                                if (pieza_recibida.Contains("crea"))
                                {
                                    //Segun el color recibido, creamos una u otra
                                    CrearPieza();
                                }
                                else  //En este caso se ha pasado a eliminar una pieza
                                {
                                    if (pieza_recibida.Contains("genera"))
                                    {
                                        GeneraObjeto();
                                    }
                                    else
                                    {
                                        //Segun el color recibido, eliminamos una u otra
                                        EliminarPieza();
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }
    }


    void CrearPieza()
    {
        GameObject elcubo = null;

        if (pieza_recibida == "creaazul")
        {
            pieza_recibida = "ninguna";
            elcubo = Instantiate(azul);

        }
        else if (pieza_recibida == "crearoja")
        {
            pieza_recibida = "ninguna";
            elcubo = Instantiate(roja);

        }
        else if (pieza_recibida == "creaamarilla")
        {
            pieza_recibida = "ninguna";
            elcubo = Instantiate(amarilla);
        }
        else if (pieza_recibida == "creamorada")
        {
            pieza_recibida = "ninguna";
            elcubo = Instantiate(morada);
        }
        else if (pieza_recibida == "creaverde")
        {
            pieza_recibida = "ninguna";
            elcubo = Instantiate(verde);
        }
        else if (pieza_recibida == "creanaranja")
        {
            pieza_recibida = "ninguna";
            elcubo = Instantiate(naranja);
        }

        elcubo.GetComponentInChildren<Canvas>().GetComponentInChildren<Text>().text = "ID: " + ID;
        elcubo.GetComponentInChildren<Canvas>().name = ID.ToString();


        ID++;
    }

    void GeneraObjeto()
    {
        string objeto = pieza_recibida.Replace("genera", "");

        //Creamos el objeto
        if(objeto != "undefined")
            FindObjectOfType<cargarArchivo>().insertarObjetoEscena(objeto);

        pieza_recibida = "ninguna";
    }

    void EliminarPieza()
    {
        //Eliminamos el GameObject que contenga un canvas con dicho nombre (es decir, su padre)
        try
        {
            Destroy(GameObject.Find(pieza_recibida).transform.parent.gameObject);
        }
        catch (NullReferenceException e) { throw; }

        pieza_recibida = "ninguna";

    }

    void CambiarEntorno()
    {
        pieza_recibida = "ninguna";
        if (Nave.activeInHierarchy)
        {
            Nave.SetActive(false);
            Warehouse.SetActive(true);
        }
        else if (Warehouse.activeInHierarchy)
        {
            Warehouse.SetActive(false);
            Fabric.SetActive(true);
        }
        else if (Fabric.activeInHierarchy)
        {
            Fabric.SetActive(false);
            Nave.SetActive(true);
        }
    }

    void TicTacToe()
    {

        PlaceObjectOnGrid grid = FindObjectOfType<PlaceObjectOnGrid>();

        if (pieza_recibida == "tictactoe1")
        {
            tictactoe.colocarTurno(1);
        }else
        {
            tictactoe.colocarTurno(2);
        }

        //Si hubiera un modo activado antes, lo eliminamos
        grid.DestroyGrid();

        //1 -> modo juego del tictactoe
        grid.CreateGrid(1);

        pieza_recibida = "ninguna";
    }

    void Guiada()
    {
        PlaceObjectOnGrid grid = FindObjectOfType<PlaceObjectOnGrid>();

        //Si hubiera un modo activado antes, lo eliminamos
        grid.DestroyGrid();

        //2 -> tetris con heuristica
        grid.CreateGrid(2);

        pieza_recibida = "ninguna";
    }

    void Libre()
    {
        PlaceObjectOnGrid grid = FindObjectOfType<PlaceObjectOnGrid>();

        //Si hubiera un modo activado antes, lo eliminamos
        grid.DestroyGrid();

        //2 -> tetris con heuristica
        grid.CreateGrid(3);

        pieza_recibida = "ninguna";
    }

    void EliminarTablero()
    {
        FindObjectOfType<PlaceObjectOnGrid>().DestroyGrid();
    }
}