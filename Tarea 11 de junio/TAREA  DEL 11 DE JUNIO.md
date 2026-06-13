**Nombre:** Wagner Maximiliano Ley Monroy

**Carnet:** 202502811

**Actividad del 11 de junio**

**PARTE 1**

**1. Estructuras de Datos Eficientes**

**Árboles Binarios de Búsqueda (ABB)**

- **Regla de ordenamiento:** Para cualquier nodo dado, todos los nodos de su **subárbol izquierdo** deben tener un valor menor, y todos los nodos de su **subárbol derecho** deben tener un valor mayor.
- **Desventaja principal (Degeneración):** Si los datos se insertan en orden secuencial (por ejemplo: 1, 2, 3, 4), el árbol no se ramifica. En su lugar, cada nodo nuevo se agrega siempre a un mismo lado, convirtiéndose en una **lista vinculada**. Esto destruye la eficiencia de la estructura, haciendo que la complejidad de búsqueda pase de $O(\log n)$ a un costoso $O(n)$.

**Árboles AVL**

- **Árbol auto-balanceado:** Es un árbol binario de búsqueda que se reestructura automáticamente (mediante rotaciones de nodos) cada vez que se inserta o elimina un elemento, asegurando que la diferencia de alturas entre subárboles se mantenga mínima.
- **Factor de balanceo:** Se calcula para cada nodo como:

$$Factor = Altura_{Izquierda} - Altura_{Derecha}$$

Para que el árbol se considere balanceado, este factor en cada nodo debe ser estrictamente **-1, 0 o 1**. Si llega a ser -2 o 2, el árbol ejecuta una rotación.

- **Complejidad $O(\log n)$:** Al garantizar por diseño que el árbol nunca se degrade y su altura ($h$) permanezca siempre proporcional a $\log n$, el camino máximo desde la raíz hasta cualquier hoja es corto. Por lo tanto, las operaciones de búsqueda, inserción y eliminación se ejecutan siempre en un tiempo máximo de $O(\log n)$.

**2. Fundamentos de Web APIs**

**API y Modelo Cliente-Servidor (Protocolo HTTP)**

- **API y Modelo:** Una API (Interfaz de Programación de Aplicaciones) es un conjunto de reglas que permite a dos softwares comunicarse. En el modelo **Cliente-Servidor**, el _Cliente_ (un navegador, app móvil, etc.) es el que solicita un recurso, y el _Servidor_ es el que almacena los datos y procesa la lógica para responder.
- **Flujo de Request y Response:** 1. El cliente genera una **Petición (Request)** HTTP que viaja por la red. Esta incluye una URL (dirección), un verbo HTTP (acción) y, a veces, cabeceras (headers) o un cuerpo con datos (body).

2. El servidor recibe la petición, la procesa y devuelve una **Respuesta (Response)** HTTP. Esta respuesta viaja de vuelta al cliente e incluye un código de estado (ej. 200 OK, 404 Not Found) y los datos solicitados (usualmente en formato JSON).

**Verbos HTTP: GET vs. POST**

| **Propiedad / Concepto**  | **GET**                                                                                                                   | **POST**                                                                                                                        |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| **Definición Conceptual** | Solicita la representación de un recurso específico.                                                                      | Envía datos al servidor para que se procesen y se cree un nuevo recurso.                                                        |
| **Uso Correcto**          | **Solo lectura.** Consultar datos sin modificar el estado del sistema (ej. ver una lista de productos).                   | **Escritura / Creación.** Enviar formularios o datos pesados que alteran la base de datos (ej. registrar un nuevo usuario).     |
| **Idempotencia**          | **Sí es idempotente.** Hacer la misma petición GET 100 veces seguidas produce el mismo resultado y no altera el servidor. | **No es idempotente.** Si envías la misma petición POST 5 veces, el servidor intentará crear 5 registros duplicados diferentes. |

Pruebas:
![[Pasted image 20260611185058.png]]![[Pasted image 20260611185404.png]]![[Pasted image 20260611185430.png]]![[Pasted image 20260611185528.png]]