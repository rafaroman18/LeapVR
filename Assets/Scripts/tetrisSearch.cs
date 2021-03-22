using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public class tetrisSearch : MonoBehaviour
{

    public PlaceObjectOnGrid grid;
    public double resultado;
    private string IDaux;

    public GameObject tetris1;
    public GameObject tetris2;
    public GameObject tetris3;
    public GameObject tetris4;
    public GameObject tetris5;
    public GameObject tetris6;
    private GameObject pieza;
    private bool gameEnd;
    private bool botonColocada = false;
    private bool retiradaPieza = false;

    public async void Tetris()
    {
        int celdas_vacias = PlaceObjectOnGrid.Squares.GetLength(0) * PlaceObjectOnGrid.Squares.GetLength(1);
        resultado = FuncionHeuristicaLibre.Heuristica(PlaceObjectOnGrid.Squares);
        botonColocada = false;
        retiradaPieza = false;
        gameEnd = false;
        pieza = null;

        grid.CambiarTexto("Heuristica: " + resultado);


        //EJECUCION DEL JUEGO
        while (!gameEnd && resultado != 0 && resultado != celdas_vacias)
        {

            //Esperará a que se termine de ejecutar 
            //la jugada del adversario
            await colocaPieza(resultado);

            //Una vez colocada, actualizamos la heuristica
            resultado = FuncionHeuristicaLibre.Heuristica(PlaceObjectOnGrid.Squares);
            grid.CambiarTexto("Heuristica: " + resultado);

        }

        if (gameEnd == true)
            resultado = -2;

        //RESULTADOS DEL JUEGO
        switch (resultado)
        {
            case 36: grid.CambiarTexto("Heuristica 36: COMPLETADO"); break;
            case 0: grid.CambiarTexto("Heuristica 0: GAME OVER"); break;
        }

    }

    public void endGame()
    {
        gameEnd = true;
        if (pieza != null)
        {
            Destroy(pieza);
            pieza = null;
        }
    }

    private void retirarPieza()
    {
        if (pieza != null)
        {
            IDaux = pieza.GetComponentInChildren<Canvas>().name;
            Destroy(pieza);
        }
        pieza = null;
    }

    public void pulsarBotonColocar()
    {
        if (pieza != null && pieza.gameObject.tag == "Colocada")
            botonColocada = true;
    }

    public void pulsarBotonRetirar()
    {
        if (pieza != null)
        {
            retiradaPieza = true;
            if (pieza.tag == "Colocada")
                FindObjectOfType<PlaceObjectOnGrid>().RemoveFromGrid(pieza.GetComponent<DetectCollision>().squareCell);
        }

    }

    private async Task<double> colocaPieza(double resultado)
    {

        while (!botonColocada && !gameEnd)
        {

            if (!retiradaPieza)
            {
                if (pieza != null && pieza.transform.position.y <= 1)
                {
                    string name = pieza.name;
                    Destroy(pieza);
                    instanciarPieza(name);
                }
            }
            else
            {
                string name = pieza.name;

                //Se elimina la pieza
                retirarPieza();

                //Se crea de nuevo
                instanciarPieza(name);

                retiradaPieza = false;
            }
            await Task.Yield();
        }

        pieza = null;
        botonColocada = false;

        return resultado;
    }


    public void asignarPieza(GameObject pieza)
    {
        this.pieza = pieza;
    }

    private void instanciarPieza(string name)
    {
        if (name.Contains("Tetris1"))
        {
            pieza = Instantiate(tetris1);
        }
        else if (name.Contains("Tetris2"))
        {
            pieza = Instantiate(tetris2);
        }
        else if (name.Contains("Tetris3"))
        {
            pieza = Instantiate(tetris3);
        }
        else if (name.Contains("Tetris4"))
        {
            pieza = Instantiate(tetris4);
        }
        else if (name.Contains("Tetris5"))
        {
            pieza = Instantiate(tetris5);
        }
        else if (name.Contains("Tetris6"))
        {
            pieza = Instantiate(tetris6);
        }

        pieza.GetComponentInChildren<Canvas>().GetComponentInChildren<Text>().text = "ID: " + IDaux;
        pieza.GetComponentInChildren<Canvas>().name = IDaux;
    }
}
