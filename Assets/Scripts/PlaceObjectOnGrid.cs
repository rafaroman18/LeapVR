using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using Leap.Unity.Interaction;
using TMPro;


public class PlaceObjectOnGrid : MonoBehaviour
{
    public Transform gridCellPrefab;
    public int rows;
    public int cols;

    public static int modo;
    public float scale;

    public static Square[,] Squares;
    public static List<Pieza> piezasColocadas;
    private Plane plane;
    private double cellSize;
    private bool botonEscalaPulsado = false;

    public GameObject TextBox;
    public GameObject botonAfirmativo;
    public GameObject botonNegativo;
    public GameObject barraEscala;


    public void Awake()
    {
        DesactivarTexto();
    }

    //////////////// FUNCIONES PÚBLICAS ////////////

    //Añadimos una pieza al tablero (si se puede)
    //Comprobamos si la pieza indicada puede colocarse
    //en el tablero y si es asi, se colocara
    public Square PlaceOnGrid(Transform obj)
    {

        Square squareCell = null;
        int angle = 0;
        int angleCorregido = 0;
        int angleZ = 0;
        bool VerdeRoja = false;
        double min = 1000000;
        double dEuclidea;
        bool resultado = false;

        //Buscamos la celda que esté más cercana a la pieza central
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < cols; ++j)
            {
                dEuclidea = euclidea(obj.position.z, obj.position.x, Squares[i, j].cellPosition.z, Squares[i, j].cellPosition.x);

                if (dEuclidea < min)
                {
                    min = dEuclidea;
                    squareCell = Squares[i, j];
                }
            }
        }

        //Si la celda a colocar está libre
        if (squareCell.isPlaceable)
        {
            //Ahora averiguamos la rotación de la pieza
            angle = Nearest(obj.eulerAngles.y);
            angleCorregido = angle;
            angleZ = Nearest(obj.eulerAngles.z);

            if ((angleZ == 90 && obj.eulerAngles.z > 0) || (angleZ == 180 && (obj.gameObject.tag == "Azul" || obj.gameObject.tag == "Naranja")))
            {
                angleCorregido = corregirAngulo(angle, angleZ, obj.gameObject.tag);
            }

            if (obj.gameObject.tag == "Morada")
            {
                resultado = Morada(squareCell, obj, angleCorregido);
            }
            else if (obj.gameObject.tag == "Amarilla")
            {
                resultado = Amarilla(squareCell, obj, angleCorregido);
            }
            else if (obj.gameObject.tag == "Verde")
            {
                if (obj.eulerAngles.z > 0 && angleZ == 90)
                {
                    VerdeRoja = true;
                    resultado = Roja(squareCell, obj, angle, angleCorregido, angleZ);
                }
                else
                {
                    angleZ = -90;
                    resultado = Verde(squareCell, obj, angle, angle, angleZ);
                }
            }
            else if (obj.gameObject.tag == "Roja")
            {
                if (obj.eulerAngles.z > 0 && angleZ == 90)
                {
                    VerdeRoja = true;
                    resultado = Verde(squareCell, obj, angle, angleCorregido, angleZ);
                }
                else
                {
                    angleZ = -90;
                    resultado = Roja(squareCell, obj, angle, angle, angleZ);
                }
            }
            else if (obj.gameObject.tag == "Azul")
            {
                resultado = Azul(squareCell, obj, angleCorregido);
            }
            else if (obj.gameObject.tag == "Naranja")
            {
                resultado = Naranja(squareCell, obj, angleCorregido);
            }
            else if (obj.gameObject.tag == "Circulo" || obj.gameObject.tag == "Cruz")
            {
                resultado = CirculoCruz(squareCell, obj, angle);
            }
        }

        //Si se ha colocado la pieza, la añadimos a la lista
        //de piezas colocadas
        if (resultado)
        {
            if (VerdeRoja)
            {
                if (obj.gameObject.tag == " Verde")
                    sumarListaPiezas(squareCell.i, squareCell.j, "Roja", angleCorregido);
                else if (obj.gameObject.tag == "Roja")
                    sumarListaPiezas(squareCell.i, squareCell.j, "Verde", angleCorregido);

            }
            else
            {
                sumarListaPiezas(squareCell.i, squareCell.j, obj.gameObject.tag, angleCorregido);
            }

            obj.gameObject.tag = "Colocada";

            if (modo == 3)
            {
                FindObjectOfType<tetrisSearch>().asignarPieza(obj.gameObject);
            }

            return squareCell;
        }
        else return null;

    }

    //Elimina una pieza del tablero
    //Recuperamos la física normal de la pieza
    //y a continuación procedemos a eliminarla del tablero
    public void RemoveFromGrid(Square squareCell)
    {

        if (squareCell != null && piezasColocadas.Count != 0)
        {
            //Eliminamos la ultima pieza del tablero

            eliminarListaPiezas(squareCell.i, squareCell.j);
        }

    }


    /*Creamos un tablero segun alguno de los modos indicados*/
    //0 -> ningun modo asignado
    //1 -> juego del 3 en raya
    //2 -> heuristica del 4x4 (tetris)
    //3 -> fuerza bruta fichero
    public void CreateGrid(int M)
    {
        //0 -> ningun modo asignado
        //1 -> juego del 3 en raya
        //2 -> heuristica del 4x4 (tetris)
        //3 -> heuristica 6x6
        switch (M)
        {
            case 1: rows = 3; cols = 3; break;
            case 2: rows = 4; cols = 4; break;
            case 3: rows = 6; cols = 6; break;
            default: rows = 4; cols = 4; break;
        }
        modo = M;
        Squares = new Square[rows, cols];
        piezasColocadas = new List<Pieza>();
        var name = 0;
        cellSize = Math.Round(gridCellPrefab.localScale.x / 0.1, 2);
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < cols; ++j)
            {
                Vector3 worldPosition = new Vector3(transform.position.x - (float)(cellSize * i), transform.position.y, transform.position.z - (float)(cellSize * j));
                Transform obj = Instantiate(gridCellPrefab, worldPosition, Quaternion.identity);
                obj.transform.eulerAngles = new Vector3(0, 0, 0);
                obj.name = "Cell " + name;
                Squares[i, j] = new Square(true, worldPosition, obj, i, j);
                name++;
            }
        }

        //Asignamos las casillas contiguas
        assignSquares();

        //Comenzamos con la simulacion
        StartMode();

    }

    public void DestroyGrid()
    {
        StopMode();

        GameObject[] celdas = GameObject.FindGameObjectsWithTag("Cell");
        GameObject[] colocadas = GameObject.FindGameObjectsWithTag("Colocada");

        for (var i = 0; i < celdas.Length; i++)
            Destroy(celdas[i]);

        for (var j = 0; j < colocadas.Length; j++)
            Destroy(colocadas[j]);
    }

    //Cambiamos el texto del cuadro de mensaje
    public void CambiarTexto(string text)
    {
        if (!TextBox.activeInHierarchy)
        {
            TextBox.SetActive(true);
        }

        TextBox.GetComponentInChildren<TextMeshProUGUI>().SetText(text);
    }

    //Comenzamos a escalar un nuevo objeto
    public async Task<int> IniciaEscala(GameObject obj)
    {
        if (!botonAfirmativo.activeInHierarchy)
        {
            botonAfirmativo.SetActive(true);
        }
        if (!barraEscala.activeInHierarchy)
        {
            barraEscala.SetActive(true);
        }

        //Esperamos a que se pulse el boton
        //confirmando la escala
        await ConfirmaEscala(obj);

        //Quitamos de la escena el boton y la barra
        botonAfirmativo.SetActive(false);
        barraEscala.SetActive(false);

        return 0;
    }

    //La barra coloca el valor de la escala
    public void CambiarEscala()
    {
        scale = barraEscala.GetComponentInChildren<InteractionSlider>().HorizontalSliderValue;
    }

    //Esta funcion indicara cuando parar el escalado
    public void pulsarBotonEscala()
    { 
        if(barraEscala.activeInHierarchy)
            botonEscalaPulsado = true; 
    }

    //Escondemos el letrero
    public void DesactivarTexto()
    {
        TextBox.SetActive(false);
    }

    ///////////////// PIEZAS ////////////////

    private bool Morada(Square cell, Transform obj, int angle)
    {
        Rigidbody rbdy = obj.GetComponent<Rigidbody>();
        InteractionBehaviour bhvr = obj.GetComponent<InteractionBehaviour>();

        bool resultado = false;

        switch (angle)
        {
            case 0:
                if (cell.isPlaceable && cell.right != null && cell.right.isPlaceable && cell.left != null
                && cell.left.isPlaceable && cell.up != null && cell.up.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 0, -90);
                    cell.isPlaceable = false;
                    cell.right.isPlaceable = false;
                    cell.left.isPlaceable = false;
                    cell.up.isPlaceable = false;
                    resultado = true;
                }
            ; break;
            case 90:
                if (cell.isPlaceable && cell.right != null && cell.right.isPlaceable && cell.up != null && cell.up.isPlaceable
                && cell.down != null && cell.down.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.down.isPlaceable = false;
                    cell.right.isPlaceable = false;
                    cell.up.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 90, -90);
                    resultado = true;

                }; break;
            case 180:
                if (cell.isPlaceable && cell.right != null && cell.right.isPlaceable && cell.left != null
                && cell.left.isPlaceable && cell.down != null && cell.down.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.right.isPlaceable = false;
                    cell.left.isPlaceable = false;
                    cell.down.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 180, -90);
                    resultado = true;

                }
            ; break;
            case 270:
                if (cell.isPlaceable && cell.left != null && cell.left.isPlaceable && cell.up != null && cell.up.isPlaceable
           && cell.down != null && cell.down.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.down.isPlaceable = false;
                    cell.left.isPlaceable = false;
                    cell.up.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 270, -90);
                    resultado = true;

                }; break;
        }

        return resultado;
    }

    private bool Amarilla(Square cell, Transform obj, int angle)
    {
        Rigidbody rbdy = obj.GetComponent<Rigidbody>();
        InteractionBehaviour bhvr = obj.GetComponent<InteractionBehaviour>();
        bool resultado = false;

        switch (angle)
        {
            case 0:

                if (cell.isPlaceable && cell.right != null && cell.right.isPlaceable && cell.down != null
                && cell.down.isPlaceable && cell.down.right != null && cell.down.right.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.right.isPlaceable = false;
                    cell.down.isPlaceable = false;
                    cell.down.right.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 0, -90);
                    resultado = true;


                }
            ; break;
            case 90:
                if (cell.isPlaceable && cell.left != null && cell.left.isPlaceable && cell.down != null && cell.down.isPlaceable
                 && cell.down.left != null && cell.down.left.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.down.isPlaceable = false;
                    cell.left.isPlaceable = false;
                    cell.down.left.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 90, -90);
                    resultado = true;


                }; break;
            case 180:
                if (cell.isPlaceable && cell.left != null && cell.left.isPlaceable && cell.up != null
                && cell.up.isPlaceable && cell.up.left != null && cell.up.left.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.up.isPlaceable = false;
                    cell.left.isPlaceable = false;
                    cell.up.left.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 180, -90);
                    resultado = true;


                }
            ; break;
            case 270:
                if (cell.isPlaceable && cell.right != null && cell.right.isPlaceable && cell.up != null && cell.up.isPlaceable
           && cell.up.right != null && cell.up.right.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.up.right.isPlaceable = false;
                    cell.right.isPlaceable = false;
                    cell.up.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 270, -90);
                    resultado = true;


                }; break;
        }

        return resultado;
    }

    private bool Verde(Square cell, Transform obj, int angle, int angleCorregido, int angleZ)
    {
        Rigidbody rbdy = obj.GetComponent<Rigidbody>();
        InteractionBehaviour bhvr = obj.GetComponent<InteractionBehaviour>();
        bool resultado = false;

        switch (angleCorregido)
        {
            case 0:

                if (cell.isPlaceable && cell.left != null && cell.left.isPlaceable && cell.down != null
                && cell.down.isPlaceable && cell.down.right != null && cell.down.right.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.left.isPlaceable = false;
                    cell.down.isPlaceable = false;
                    cell.down.right.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, angle, angleZ);
                    resultado = true;


                }
            ; break;
            case 90:
                if (cell.isPlaceable && cell.left != null && cell.left.isPlaceable && cell.up != null && cell.up.isPlaceable
                 && cell.left.down != null && cell.left.down.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.up.isPlaceable = false;
                    cell.left.isPlaceable = false;
                    cell.left.down.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, angle, angleZ);
                    resultado = true;


                }; break;
            case 180:
                if (cell.isPlaceable && cell.right != null && cell.right.isPlaceable && cell.up != null
                && cell.up.isPlaceable && cell.up.left != null && cell.up.left.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.up.isPlaceable = false;
                    cell.right.isPlaceable = false;
                    cell.up.left.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, angle, angleZ);
                    resultado = true;


                }
            ; break;
            case 270:
                if (cell.isPlaceable && cell.right != null && cell.right.isPlaceable && cell.down != null && cell.down.isPlaceable
           && cell.up.right != null && cell.up.right.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.up.right.isPlaceable = false;
                    cell.right.isPlaceable = false;
                    cell.down.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, angle, angleZ);
                    resultado = true;


                }; break;
        }

        return resultado;

    }

    private bool Roja(Square cell, Transform obj, int angle, int angleCorregido, int angleZ)
    {
        Rigidbody rbdy = obj.GetComponent<Rigidbody>();
        InteractionBehaviour bhvr = obj.GetComponent<InteractionBehaviour>();
        bool resultado = false;

        switch (angleCorregido)
        {
            case 0:

                if (cell.isPlaceable && cell.right != null && cell.right.isPlaceable && cell.down != null
                && cell.down.isPlaceable && cell.down.left != null && cell.down.left.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.right.isPlaceable = false;
                    cell.down.isPlaceable = false;
                    cell.down.left.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, angle, angleZ);
                    resultado = true;


                }
            ; break;
            case 90:
                if (cell.isPlaceable && cell.left != null && cell.left.isPlaceable && cell.down != null && cell.down.isPlaceable
                 && cell.left.up != null && cell.left.up.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.down.isPlaceable = false;
                    cell.left.isPlaceable = false;
                    cell.left.up.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, angle, angleZ);
                    resultado = true;


                }; break;
            case 180:
                if (cell.isPlaceable && cell.left != null && cell.left.isPlaceable && cell.up != null
                && cell.up.isPlaceable && cell.up.right != null && cell.up.right.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.up.isPlaceable = false;
                    cell.left.isPlaceable = false;
                    cell.up.right.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, angle, angleZ);
                    resultado = true;


                }
            ; break;
            case 270:
                if (cell.isPlaceable && cell.right != null && cell.right.isPlaceable && cell.up != null && cell.up.isPlaceable
           && cell.down.right != null && cell.down.right.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.down.right.isPlaceable = false;
                    cell.right.isPlaceable = false;
                    cell.up.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, angle, angleZ);
                    resultado = true;


                }; break;
        }

        return resultado;

    }

    private bool Azul(Square cell, Transform obj, int angle)
    {
        Rigidbody rbdy = obj.GetComponent<Rigidbody>();
        InteractionBehaviour bhvr = obj.GetComponent<InteractionBehaviour>();
        bool resultado = false;

        switch (angle)
        {
            case 90:
                if (cell.isPlaceable && cell.left != null && cell.left.isPlaceable && cell.left.left != null
                && cell.left.left.isPlaceable && cell.left.left.left != null && cell.left.left.left.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.left.isPlaceable = false;
                    cell.left.left.isPlaceable = false;
                    cell.left.left.left.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 90, 0);
                    resultado = true;


                }
            ; break;
            case 180:
                if (cell.isPlaceable && cell.up != null && cell.up.isPlaceable && cell.up.up != null && cell.up.up.isPlaceable
                && cell.up.up.up != null && cell.up.up.up.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.up.isPlaceable = false;
                    cell.up.up.isPlaceable = false;
                    cell.up.up.up.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 180, 0);
                    resultado = true;


                }; break;
            case 270:
                if (cell.isPlaceable && cell.right != null && cell.right.isPlaceable && cell.right.right != null
                && cell.right.right.isPlaceable && cell.right.right.right != null && cell.right.right.right.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.right.isPlaceable = false;
                    cell.right.right.isPlaceable = false;
                    cell.right.right.right.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 270, 0);
                    resultado = true;


                }
            ; break;
            case 0:
                if (cell.isPlaceable && cell.down != null && cell.down.isPlaceable && cell.down.down != null && cell.down.down.isPlaceable
                && cell.down.down.down != null && cell.down.down.down.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.down.isPlaceable = false;
                    cell.down.down.isPlaceable = false;
                    cell.down.down.down.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 0, 0);
                    resultado = true;


                }; break;
        }

        return resultado;

    }

    private bool Naranja(Square cell, Transform obj, int angle)
    {
        Rigidbody rbdy = obj.GetComponent<Rigidbody>();
        InteractionBehaviour bhvr = obj.GetComponent<InteractionBehaviour>();
        bool resultado = false;

        switch (angle)
        {
            case 0:

                if (cell.isPlaceable && cell.right != null && cell.right.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.right.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 0, 0);
                    resultado = true;

                }
            ; break;
            case 90:
                if (cell.isPlaceable && cell.down != null && cell.down.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.down.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 90, 0);
                    resultado = true;

                }; break;
            case 180:
                if (cell.isPlaceable && cell.left != null && cell.left.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.left.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 180, 0);
                    resultado = true;


                }
            ; break;
            case 270:
                if (cell.isPlaceable && cell.up != null && cell.up.isPlaceable)
                {
                    MagnetizarPieza(rbdy, obj, bhvr);
                    cell.isPlaceable = false;
                    cell.up.isPlaceable = false;
                    obj.position = cell.cellPosition + new Vector3(0, 0.08f, 0);
                    obj.eulerAngles = new Vector3(0, 270, 0);
                    resultado = true;
                }; break;
        }

        return resultado;

    }

    private bool CirculoCruz(Square cell, Transform obj, int angle)
    {
        Rigidbody rbdy = obj.GetComponent<Rigidbody>();
        InteractionBehaviour bhvr = obj.GetComponent<InteractionBehaviour>();
        bool resultado = false;

        if (cell.isPlaceable == true)
        {
            MagnetizarPieza(rbdy, obj, bhvr);
            cell.isPlaceable = false;
            obj.position = cell.cellPosition + new Vector3(0, 0.04f, 0);
            obj.eulerAngles = new Vector3(0, 0, 0);
            resultado = true;
        }



        return resultado;
    }

    private void MagnetizarPieza(Rigidbody rbdy, Transform obj, InteractionBehaviour behaviour)
    {
        behaviour.ignoreContact = true;
        behaviour.ignoreGrasping = true;
        behaviour.ReleaseFromGrasp();
        rbdy.isKinematic = true;
        rbdy.detectCollisions = false;
    }

    ///////////////// FUNCIONES AUXILIARES ////////////

    //Devuelve el grado mas cercano 
    //entre 0,90,180 o 270 grados
    private int Nearest(float Y)
    {
        int angle = (int)Math.Round(Y);
        angle = angle % 360;
        int valor1, valor2, valor3, valor4, min, nearest;

        valor1 = Math.Abs(angle - 90);
        valor2 = Math.Abs(angle - 180);
        valor3 = Math.Abs(angle - 270);
        valor4 = Math.Abs(angle - 360);

        min = Math.Min((int)Y, Math.Min(Math.Min(valor1, valor4), Math.Min(valor2, valor3)));

        if (min == valor4 || min == Y)
        {
            nearest = 0;
        }
        else if (min == valor1)
        {
            nearest = 90;
        }
        else if (min == valor2)
        {
            nearest = 180;
        }
        else if (min == valor3)
        {
            nearest = 270;
        }
        else
        {
            nearest = 0;
        }

        return nearest;
    }

    //Corrije el angulo Y respecto al que
    //posea el objeto en Z
    private int corregirAngulo(int angle, int angleZ, string tag)
    {
        int resultado = angle;

        if (tag == "Morada" && angleZ == 90)
        {
            switch (angle)
            {
                case 0: resultado = 180; break;
                case 90: resultado = 270; break;
                case 180: resultado = 0; break;
                case 270: resultado = 90; break;
            }
        }
        else if (tag == "Amarilla" && angleZ == 90)
        {
            switch (angle)
            {
                case 0: resultado = 270; break;
                case 90: resultado = 0; break;
                case 180: resultado = 90; break;
                case 270: resultado = 180; break;
            }
        }
        else if (tag == "Verde" && angleZ == 90)
        {

            switch (angle)
            {
                case 0: resultado = 180; break;
                case 90: resultado = 270; break;
                case 180: resultado = 0; break;
                case 270: resultado = 90; break;
            }

        }
        else if (tag == "Roja" && angleZ == 90)
        {
            switch (angle)
            {
                case 0: resultado = 180; break;
                case 90: resultado = 270; break;
                case 180: resultado = 0; break;
                case 270: resultado = 90; break;
            }
        }

        if ((tag == "Azul" || tag == "Naranja") && angleZ == 180)
        {
            switch (angle)
            {
                case 0: resultado = 180; break;
                case 90: resultado = 270; break;
                case 180: resultado = 0; break;
                case 270: resultado = 90; break;
            }
        }


        return resultado;
    }


    //Asigna a cada una de las celdas
    //una referencia de las celdas adyacentes
    private void assignSquares()
    {
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < cols; ++j)
            {

                //Asignamos celda de arriba
                if (i > 0)
                {
                    Squares[i, j].up = Squares[i - 1, j];
                }

                //Asignamos celda derecha
                if (j != (cols - 1))
                {
                    Squares[i, j].right = Squares[i, j + 1];
                }

                //Asignamos celda izquierda
                if (j > 0)
                {
                    Squares[i, j].left = Squares[i, j - 1];
                }

                //Asignamos celda de abajo
                if (i != (rows - 1))
                {
                    Squares[i, j].down = Squares[i + 1, j];
                }
            }
        }
    }


    //Devuelve distancia euclidea
    private double euclidea(double xA, double yA, double xB, double yB)
    {
        return Math.Sqrt(Math.Pow(xB - xA, 2) + Math.Pow(yB - yA, 2));
    }


    //Cambiamos el valor de la escala del objeto generado
    private async Task<int> ConfirmaEscala(GameObject obj)
    {
        while (!botonEscalaPulsado)
        {
            if (obj.transform.localScale.x != scale)
            {
                obj.transform.localScale = new Vector3(scale, scale, scale);
            }
            await Task.Yield();
        }

        scale = barraEscala.GetComponentInChildren<InteractionSlider>().HorizontalSliderValue;
        obj.transform.localScale = new Vector3(scale, scale, scale);
        botonEscalaPulsado = false;

        return 0;

    }


    //Una vez que una pieza se haya colocado
    //correctamente, la añadimos a la lista
    private void sumarListaPiezas(int i, int j, string tag, int angle)
    {
        //tipo-> numero de piezas * 4 angulos
        int tipo = -1;

        switch (tag)
        {
            case "Morada":
                switch (angle)
                {
                    case 0: tipo = 0; ; break;
                    case 90: tipo = 1; ; break;
                    case 180: tipo = 2; ; break;
                    case 270: tipo = 3; ; break;
                }
            ; break;
            case "Amarilla":
                switch (angle)
                {
                    case 0: tipo = 4; ; break;
                    case 90: tipo = 5; ; break;
                    case 180: tipo = 6; ; break;
                    case 270: tipo = 7; ; break;
                }; break;
            case "Verde":
                switch (angle)
                {
                    case 0: tipo = 8; ; break;
                    case 90: tipo = 9; ; break;
                    case 180: tipo = 10; ; break;
                    case 270: tipo = 11; ; break;
                }; break;
            case "Roja":
                switch (angle)
                {
                    case 0: tipo = 12; ; break;
                    case 90: tipo = 13; ; break;
                    case 180: tipo = 14; ; break;
                    case 270: tipo = 15; ; break;
                }; break;
            case "Azul":
                switch (angle)
                {
                    case 90: tipo = 16; ; break;
                    case 180: tipo = 17; ; break;
                    case 270: tipo = 18; ; break;
                    case 0: tipo = 19; ; break;
                }; break;
            case "Naranja":
                switch (angle)
                {
                    case 0: tipo = 20; ; break;
                    case 90: tipo = 21; ; break;
                    case 180: tipo = 22; ; break;
                    case 270: tipo = 23; ; break;
                }; break;

            case "Circulo": tipo = 24; break;
            case "Cruz": tipo = 25; break;
        }

        Pieza pieza = new Pieza();
        pieza.i = i;
        pieza.j = j;
        pieza.tipo = tipo;
        piezasColocadas.Add(pieza);

    }

    //Si la pieza se ha retirado del tablero, 
    //es eliminada de la lista de Piezas
    //y del tablero
    private void eliminarListaPiezas(int i, int j)
    {
        bool eliminada = false;
        int tipo = 0;
        Pieza p;

        for (int x = 0; x < piezasColocadas.Count && !eliminada; ++x)
        {
            p = piezasColocadas[x];

            if (p.i == i && p.j == j)
            {
                tipo = p.tipo;
                piezasColocadas.Remove(p);
                eliminada = true;

                Square cell = Squares[i, j];

                try
                {
                    //Ahora eliminamos la pieza del tablero
                    switch (tipo)
                    {
                        //MORADA
                        case 0: cell.isPlaceable = cell.up.isPlaceable = cell.right.isPlaceable = cell.left.isPlaceable = true; break;
                        case 1: cell.isPlaceable = cell.down.isPlaceable = cell.right.isPlaceable = cell.up.isPlaceable = true; break;
                        case 2: cell.isPlaceable = cell.right.isPlaceable = cell.left.isPlaceable = cell.down.isPlaceable = true; break;
                        case 3: cell.isPlaceable = cell.down.isPlaceable = cell.left.isPlaceable = cell.up.isPlaceable = true; break;

                        //AMARILLA
                        case 4: cell.isPlaceable = cell.right.isPlaceable = cell.down.isPlaceable = cell.down.right.isPlaceable = true; break;
                        case 5: cell.isPlaceable = cell.down.isPlaceable = cell.left.isPlaceable = cell.down.left.isPlaceable = true; break;
                        case 6: cell.isPlaceable = cell.up.isPlaceable = cell.left.isPlaceable = cell.up.left.isPlaceable = true; break;
                        case 7: cell.isPlaceable = cell.up.right.isPlaceable = cell.right.isPlaceable = cell.up.isPlaceable = true; break;

                        //VERDE
                        case 8: cell.isPlaceable = cell.left.isPlaceable = cell.down.isPlaceable = cell.down.right.isPlaceable = true; break;
                        case 9: cell.isPlaceable = cell.up.isPlaceable = cell.left.isPlaceable = cell.left.down.isPlaceable = true; break;
                        case 10: cell.isPlaceable = cell.up.isPlaceable = cell.right.isPlaceable = cell.up.left.isPlaceable = true; break;
                        case 11: cell.isPlaceable = cell.up.right.isPlaceable = cell.right.isPlaceable = cell.down.isPlaceable = true; break;

                        //ROJA
                        case 12: cell.isPlaceable = cell.right.isPlaceable = cell.down.isPlaceable = cell.down.left.isPlaceable = true; break;
                        case 13: cell.isPlaceable = cell.down.isPlaceable = cell.left.isPlaceable = cell.left.up.isPlaceable = true; break;
                        case 14: cell.isPlaceable = cell.up.isPlaceable = cell.left.isPlaceable = cell.up.right.isPlaceable = true; break;
                        case 15: cell.isPlaceable = cell.down.right.isPlaceable = cell.right.isPlaceable = cell.up.isPlaceable = true; break;

                        //AZUL
                        case 16: cell.isPlaceable = cell.left.isPlaceable = cell.left.left.isPlaceable = cell.left.left.left.isPlaceable = true; break;
                        case 17: cell.isPlaceable = cell.up.isPlaceable = cell.up.up.isPlaceable = cell.up.up.up.isPlaceable = true; break;
                        case 18: cell.isPlaceable = cell.right.isPlaceable = cell.right.right.isPlaceable = cell.right.right.right.isPlaceable = true; break;
                        case 19: cell.isPlaceable = cell.down.isPlaceable = cell.down.down.isPlaceable = cell.down.down.down.isPlaceable = true; break;

                        //NARANJA
                        case 20: cell.isPlaceable = cell.right.isPlaceable = true; break;
                        case 21: cell.isPlaceable = cell.down.isPlaceable = true; break;
                        case 22: cell.isPlaceable = cell.left.isPlaceable = true; break;
                        case 23: cell.isPlaceable = cell.up.isPlaceable = true; break;

                        //CIRCULO
                        case 24: cell.isPlaceable = true; break;

                        //CRUZ
                        case 25: cell.isPlaceable = true; break;

                    }
                }
                catch (NullReferenceException e) { }
            }
        }
    }

    //Aqui marcaremos la diferencia de si es 
    //3 en raya o fuerza bruta o heuristica
    public void StartMode()
    {
        //0 -> ningun modo asignado
        //1 -> juego del 3 en raya
        //2 -> heuristica del 4x4 (tetris)
        //3 -> fuerza bruta fichero
        switch (modo)
        {
            case 1:
                GetComponent<tictactoe>().TicTacToe(); break;
            case 2:
                GetComponent<tetrisHeuristic>().Tetris(); break;

            case 3:
                GetComponent<tetrisSearch>().Tetris(); break;
        }

        botonAfirmativo.SetActive(true);
        botonNegativo.SetActive(true);
        TextBox.SetActive(true);

    }


    //Esta funcion parará el modo actual que se esté ejecutando
    private void StopMode()
    {
        //0 -> ningun modo asignado
        //1 -> juego del 3 en raya
        //2 -> heuristica del 4x4 (tetris)
        //3 -> fuerza bruta fichero
        switch (modo)
        {
            case 1:
                GetComponent<tictactoe>().endGame(); break;
            case 2:
                GetComponent<tetrisHeuristic>().endGame(); break;

            case 3:
                GetComponent<tetrisSearch>().endGame(); break;
        }

        botonAfirmativo.SetActive(false);
        botonNegativo.SetActive(false);
        TextBox.SetActive(false);

    }


}

///////////////// CLASE SQUARE //////////////

public class Square
{
    public bool isPlaceable;
    public Vector3 cellPosition;
    public Transform obj;
    public int i;
    public int j;

    public Square up;
    public Square left;
    public Square right;
    public Square down;

    public Square(bool isPlaceable, Vector3 cellPosition, Transform obj, int i, int j)
    {
        this.isPlaceable = isPlaceable;
        this.cellPosition = cellPosition;
        this.obj = obj;
        this.i = i;
        this.j = j;
        up = left = right = down = null;
    }

}
