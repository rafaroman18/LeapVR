using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class tetrisHeuristic : MonoBehaviour
{

    public PlaceObjectOnGrid grid;
    public GameObject tetris1;
    public GameObject tetris2;
    public GameObject tetris3;
    public GameObject tetris4;
    public GameObject tetris5;
    public GameObject tetris6;
    public double resultado;

    private static System.Random rnd = new System.Random();
    private GameObject pieza1;
    private int piezas_totales;
    private bool gameEnd;
    private bool botonColocada = false;
    private bool retiradaPieza = false;

    public async void Tetris()
    {
        piezas_totales = 0;
        int celdas_vacias = PlaceObjectOnGrid.Squares.GetLength(0) * PlaceObjectOnGrid.Squares.GetLength(1);
        resultado = FuncionHeuristica.Heuristica(PlaceObjectOnGrid.Squares);
        botonColocada = false;
        retiradaPieza = false;
        gameEnd = false;

        grid.CambiarTexto("Heuristica: " + resultado);


        //EJECUCION DEL JUEGO
        while (!gameEnd && resultado != 0 && resultado != celdas_vacias)
        {

            //Esperará a que se termine de ejecutar 
            //la jugada del adversario
            await colocaPieza(resultado);

            //Una vez colocada, actualizamos la heuristica
            resultado = FuncionHeuristica.Heuristica(PlaceObjectOnGrid.Squares);
            grid.CambiarTexto("Heuristica: " + resultado);

        }

        if (gameEnd == true)
            resultado = -2;

        //RESULTADOS DEL JUEGO
        switch (resultado)
        {
            case 16: grid.CambiarTexto("Heuristica 16: COMPLETADO"); break;
            case 0: grid.CambiarTexto("Heuristica 0: GAME OVER"); break;
        }

    }

    public void endGame()
    {
        gameEnd = true;
        if (pieza1 != null)
        {
            Destroy(pieza1);
            pieza1 = null;
        }
    }

    private void retirarPieza()
    {
        if (pieza1 != null)
            Destroy(pieza1);
        pieza1 = null;
    }

    public void pulsarBotonColocar()
    {
        if (pieza1 != null && pieza1.gameObject.tag == "Colocada")
            botonColocada = true;
    }

    public void pulsarBotonRetirar()
    {
        if (pieza1 != null)
        {
            retiradaPieza = true;
            if (pieza1.gameObject.tag == "Colocada")
                FindObjectOfType<PlaceObjectOnGrid>().RemoveFromGrid(pieza1.GetComponent<DetectCollision>().squareCell);
        }

    }

    private async Task<double> colocaPieza(double resultado)
    {

        instanciarPieza(pieza1);

        while (!botonColocada && !gameEnd)
        {
            if (!retiradaPieza)
            {
                if (pieza1 != null && pieza1.transform.position.y <= 1)
                {
                    Destroy(pieza1);
                    instanciarPieza(pieza1);
                }
            }
            else
            {
                //Se elimina la pieza
                retirarPieza();

                //Se crea de nuevo
                instanciarPieza(pieza1);

                retiradaPieza = false;
            }
            await Task.Yield();
        }

        botonColocada = false;

        piezas_totales++;
        return resultado;
    }


    private void instanciarPieza(GameObject pieza)
    {
        switch (piezas_totales)
        {
            case 0: pieza1 = Instantiate(tetris1); break;
            case 1: pieza1 = Instantiate(tetris6); break;
            case 2: pieza1 = Instantiate(tetris4); break;
            case 3: pieza1 = Instantiate(tetris6); break;
            case 4: pieza1 = Instantiate(tetris1); break;
        }
        pieza1.GetComponentInChildren<Canvas>().enabled = false;
    }

    

    }
