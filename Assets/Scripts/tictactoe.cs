using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity;
using UnityEngine;

public class tictactoe : MonoBehaviour
{
    public PlaceObjectOnGrid grid;
    private static int N = 9;
    private static int[] tablero_inicial = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private static int[,] opciones_victoria = { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 }, { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 }, { 0, 4, 8 }, { 2, 4, 6 } };

    private tNodo juego;
    private static int primero;

    public GameObject Cruz;
    public GameObject Circulo;
    private GameObject actualGameObject;

    public AudioSource sonidoCorrecto;

    private GameObject pieza;
    private bool botonColocada = false;
    private bool retiradaPieza = false;
    private bool gameEnd;
    ///////////////// FUNCIONES PUBLICAS ////////////////

    /*Se realiza el juego del TicTacToe entre el usuario y la maquina*/
    public async void TicTacToe()
    {
        int jugador;
        int ganador;
        gameEnd = false;
        botonColocada = false;
        retiradaPieza = false;

        juego = crearNodo(tablero_inicial);

        grid.CambiarTexto("La IA juega con X \nUsted con O");

        if (primero == 1)
            jugador = -1; //Humano (O) - Min
        else
            jugador = 1; //IA (X) - Max

        ganador = terminal(juego);

        //EJECUCION DEL JUEGO
        while (!gameEnd && juego.vacias > 0 && ganador == 0)
        {
            if (jugador == 1)
                juego = minimax(juego, jugador);
            else
                //Esperará a que se termine de ejecutar 
                //la jugada del adversario
                await jugadaAdversario(juego);

            ganador = terminal(juego);

            jugador = opuesto(jugador);
        }

        if (gameEnd)
            ganador = -2;

        //RESULTADOS DEL JUEGO
        switch (ganador)
        {
            case 1: grid.CambiarTexto("Gana la IA"); break;
            case -1: grid.CambiarTexto("Gana el HUMANO"); break;
            case 0: grid.CambiarTexto("Empate"); break;
        }

    }

    /*Especificamos si queremos empezar primero en el juego o no
    Valores: 1 -> empieza el jugador
             Cualquier otro -> empieza la maquina*/
    public static void colocarTurno(int pr)
    {
        primero = pr;
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
            Destroy(pieza);
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
            if (pieza.gameObject.tag == "Colocada")
                FindObjectOfType<PlaceObjectOnGrid>().RemoveFromGrid(pieza.GetComponent<DetectCollision>().squareCell);
        }

    }

    ///////////////// FUNCIONES CLAVE MINIMAX ////////////////
    private static tNodo crearNodo(int[] tablero)
    {
        tNodo resultado = new tNodo();
        for (int i = 0; i < N; ++i)
        {
            resultado.celdas[i] = tablero[i];
            if (tablero[i] == 0)
                resultado.vacias++;
        }

        return resultado;
    }

    private static int terminal(tNodo juego)
    {
        int i = 0; int res = 0;
        while (res == 0 && i < 8)
        {
            if (juego.celdas[opciones_victoria[i, 0]] != 0 &&
            juego.celdas[opciones_victoria[i, 0]] == juego.celdas[opciones_victoria[i, 1]] &&
            juego.celdas[opciones_victoria[i, 1]] == juego.celdas[opciones_victoria[i, 2]])
            {
                res = juego.celdas[opciones_victoria[i, 2]];
            }

            i++;
        }

        return res;
    }

    private static int opuesto(int jugador)
    {
        if (jugador == 1)
            return -1;
        else
            return 1;
    }

    private async Task<tNodo> jugadaAdversario(tNodo t)
    {
        pieza = Instantiate(Circulo);

        //Esperamos hasta que el usuario haya colocado la pieza
        while (!botonColocada && !gameEnd)
        {
            if (!retiradaPieza)
            {
                if (pieza.transform.position.y <= 1)
                {
                    Destroy(pieza);
                    pieza = Instantiate(Circulo);
                }
            }
            else
            {
                //Se elimina la pieza de Unity
                retirarPieza();

                //Se crea de nuevo la de Unity
                pieza = Instantiate(Circulo);

                retiradaPieza = false;
            }
            await Task.Yield();

        };

        if (!gameEnd)
        {

            botonColocada = false;

            //Una vez colocada, actualizamos los valores del
            //tablero de la IA
            int i = pieza.GetComponent<DetectCollision>().squareCell.i;
            int j = pieza.GetComponent<DetectCollision>().squareCell.j;

            int jugada = i * PlaceObjectOnGrid.Squares.GetLength(0) + j;

            juego = aplicaJugada(t, -1, jugada);
        }

        return juego;

    }

    private bool esValida(tNodo actual, int jugada)
    {
        return (jugada >= 0 && jugada <= 9 && actual.celdas[jugada] == 0);
    }

    private tNodo aplicaJugada(tNodo t, int jugador, int jugada)
    {
        tNodo resultado = t.clone();
        resultado.celdas[jugada] = jugador;
        resultado.vacias--;
        return resultado;
    }

    ///////////////// MINIMAX ////////////////
    private tNodo minimax(tNodo nodo, int jugador)
    {
        int max, max_actual, jugada, mejorJugada = 0;
        tNodo intento;
        max = -1000;
        for (jugada = 0; jugada < N; ++jugada)
        {
            if (esValida(nodo, jugada))
            {
                intento = aplicaJugada(nodo, jugador, jugada);
                max_actual = valorMin(intento);
                if (max_actual > max)
                {
                    max = max_actual;
                    mejorJugada = jugada;
                }
            }
        }

        //Colocamos la pieza en el tablero de Unity
        int i = mejorJugada / PlaceObjectOnGrid.Squares.GetLength(0);
        int j = mejorJugada % PlaceObjectOnGrid.Squares.GetLength(0);

        GameObject cruz = Instantiate(Cruz);
        cruz.transform.position = PlaceObjectOnGrid.Squares[i, j].obj.transform.position + new Vector3(0, 0.2f, 0);

        //Reproducimos un sonido indicando el principio del turno 
        //de nuevo
        sonidoCorrecto.Play();

        return aplicaJugada(nodo, jugador, mejorJugada);
    }
    private int valorMin(tNodo nodo)
    {
        int valor_min, jugada, jugador = -1, ganador;
        ganador = terminal(nodo);

        if (ganador == 0 && nodo.vacias > 0)
        {
            valor_min = +1000;
            for (jugada = 0; jugada < N; ++jugada)
            {
                if (esValida(nodo, jugada))
                {
                    valor_min = Math.Min(valor_min, valorMax(aplicaJugada(nodo, jugador, jugada)));
                }
            }
            ganador = valor_min;
        }

        return ganador;
    }
    private int valorMax(tNodo nodo)
    {
        int valor_max, jugada, jugador = 1, ganador;
        ganador = terminal(nodo);

        if (ganador == 0 && nodo.vacias > 0)
        {
            valor_max = -1000;
            for (jugada = 0; jugada < N; jugada++)
            {
                if (esValida(nodo, jugada))
                {
                    valor_max = Math.Max(valor_max, valorMin(aplicaJugada(nodo, jugador, jugada)));
                }
            }
            ganador = valor_max;
        }
        return ganador;
    }

    ///////////////// CLASE TNODO ////////////////
    private class tNodo
    {
        public int[] celdas;
        public int vacias;

        public tNodo()
        {
            celdas = new int[N];
            vacias = 0;
        }

        public tNodo clone()
        {
            tNodo res = new tNodo();
            res.vacias = vacias;
            for (int i = 0; i < N; ++i)
                res.celdas[i] = celdas[i];
            return res;
        }
    }
}