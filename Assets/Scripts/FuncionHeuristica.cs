public class FuncionHeuristica
{
    public static double Heuristica(Square[,] celdas)
    {
        int celdas_vacias = 0;
        bool aislada = false;
        double resultado = 0.0;

        for (int i = 0; i < celdas.GetLength(0) && !aislada; ++i)
        {
            for (int j = 0; j < celdas.GetLength(1) && !aislada; ++j)
            {
                if (celdas[i, j].isPlaceable)
                {
                    celdas_vacias++;
                    if (celdaAislada(celdas[i, j]))
                    {
                        aislada = true;
                    }
                }
            }
        }

        if (celdas_vacias == celdas.GetLength(0)*celdas.GetLength(1)) //El estado inicial tiene heuristica 1
        {
            resultado = 1;
        }
        else
        {
            if (!aislada)
            {
                resultado = (celdas.GetLength(0) * celdas.GetLength(1)) - celdas_vacias;
            }
        }

        return resultado;
    }

    private static bool celdaAislada(Square celda)
    {
        bool aislada = false;

        if ((celda.left == null || !celda.left.isPlaceable) &&
        (celda.right == null || !celda.right.isPlaceable) &&
        (celda.up == null || !celda.up.isPlaceable) &&
        (celda.down == null || !celda.down.isPlaceable))
            aislada = true;

        return aislada;
    }

}
