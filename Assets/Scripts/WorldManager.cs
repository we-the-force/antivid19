﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldManager : MonoBehaviour
{
    public static WorldManager instance;

    private void Awake()
    {
        instance = this;
    }

    public Transform NodeParent;
    public List<PathFindingNode> NodeCollection;


    // Start is called before the first frame update
    void Start()
    {
        NodeCollection = new List<PathFindingNode>();

        for (int i = 0; i < NodeParent.childCount; i++)
        {
            PathFindingNode _node = NodeParent.GetChild(i).GetComponent<PathFindingNode>();
            _node.NodeID = i; 
            _node.InitConnections();
            NodeCollection.Add(_node);
        }
        InitPathCollection();
    }


    public void InitPathCollection()
    {
        //--- Ordena tanto los nodos, como las conexiones entre ellos en cada nodo
        NodeCollection = NodeCollection.OrderBy(x => x.NodeID).ToList();

        foreach (PathFindingNode obj in NodeCollection)
        {
            obj.NodeConnection = obj.NodeConnection.OrderBy(x => x.ConnectedNode.NodeID).ToList();
        }
        //---------------------------------------------------------------------------

        int rank = NodeCollection.Count;

        //  Debug.LogError(" >>> TOTAL DE NODOS >>> " + rank);

        int[,] connectionArray = new int[rank, rank];
        string[] ids = new string[rank];

        int row = 0;
        int colDisplacement = 0;
        PathFindingNode rowObj;
        PathFindingNode colObj;

        for (int node = 0; node < rank; node++)
        {
            //--- Este ciclo recorre cada nodo del sistema de busqueda
            //--- y le asigna el camino mas corto a los otros nodos 
            row = node;

            colDisplacement = node;

            //--- Resetear el connection array para llenarlo nuevamente
            ResetConnectionArray(out connectionArray, rank);

            //--- RECORRE NUEVAMENTE TODOS LOS NODOS
            //--- EMPEZANDO DESDE EL NODO SIGUIENTE SELECCIONADO
            for (int i = 0; i < rank; i++)
            {
                rowObj = NodeCollection[i];

                for (int k = 0; k < rowObj.ConnectedNodes.Count; k++)
                {
                    colObj = rowObj.NodeConnection[k].ConnectedNode;

                    int currentRow = rowObj.NodeID - colDisplacement;
                    int currentCol = colObj.NodeID - colDisplacement;
                    if (currentRow < 0)
                    {
                        currentRow = rank + currentRow;  //--- al ser negativo, se resta
                    }
                    if (currentCol < 0)
                    {
                        currentCol = rank + currentCol;  //--- al ser negativo, se resta
                    }

                    //     Debug.LogError("COL: " + currentCol + " ROW: " + currentRow);

                    connectionArray[currentRow, currentCol] = 1;
                }

                ids[i] = row.ToString();

                row++;
                if (row == rank)
                {
                    row = 0;
                }
            }

            List<string> lista = Dijkstra.Instance.DijkstraInit(rank, connectionArray, ids);

            rowObj = NodeCollection[row];
            for (int i = 0; i < lista.Count; i++)
            {
                rowObj.ShortestPathCollection.Add(lista[i]);
            }
        }
    }

    private void ResetConnectionArray(out int[,] connectionArray, int rank)
    {
        //  Debug.LogError(" >>> Reseteando arreglo: " + rank);

        connectionArray = new int[rank, rank];

        for (int i = 0; i < rank; i++)
        {
            for (int k = 0; k < rank; k++)
            {
                connectionArray[i, k] = -1;
            }
        }
    }


    /// <summary>
    /// ANTON ::
    /// Este metodo se usara para que los agentes puedan conocer cual es el siguiente punto en el 
    /// arreglo de tiles al que deben moverse, regresara un valor de Vector2, que representa el 
    /// punto exacto en el espacio local a donde se movera el agente.
    /// 
    /// De igual forma el calculo por default lo hace buscando al personaje, pero tiene  la opcion
    /// de recibir un segundo valor en forma de Vector2, con lo cual lo utilizara para encontrar la
    /// ruta mas corta hacia esa segunda posicion
    /// </summary>
    /// <param name="myPosition">La posicion del agente que solicita una ruta</param>
    /// <param name="findPlayer">TRUE: Por default busca al jugador ; FALSE : busca en referencia a somePosition</param>
    /// <param name="somePosition">Posicion asignada a partir de la cual realiza la busqueda</param>
    /// <returns></returns>
    //public Transform GetNextTileInRoute(Transform myPosition, Transform somePosition = null)
    public PathFindingNode GetNextTileInRoute(int myNodeID, int nextNodeID)
    {
        PathFindingNode result = null;
        PathFindingNode tileInfoOrigin = null;
        PathFindingNode tileInfoDestination = null;
        string destinationId;
        string[] pathIdCollection;

        for (int i = 0; i < NodeCollection.Count; i++)
        {
            if (NodeCollection[i].NodeID == myNodeID)
                tileInfoOrigin = NodeCollection[i];

            if (NodeCollection[i].NodeID == nextNodeID)
                tileInfoDestination = NodeCollection[i];
        }

        destinationId = nextNodeID.ToString();
        for (int i = 0; i < tileInfoOrigin.ShortestPathCollection.Count; i++)
        {
            pathIdCollection = tileInfoOrigin.ShortestPathCollection[i].Split(',');
            if (pathIdCollection[0] == destinationId)
            {
                destinationId = pathIdCollection[pathIdCollection.Length - 1];
                break;
            }
        }

        for (int i = 0; i < NodeCollection.Count; i++)
        {
            if (NodeCollection[i].NodeID.ToString() == destinationId)
            {
                result = NodeCollection[i];
                break;
            }
        }

        return result;
    }

}
