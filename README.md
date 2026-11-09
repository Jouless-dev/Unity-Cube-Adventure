# Cube Adventure

Prototipo de plataformas 2.5D desarrollado en Unity 6 (versión 6000.6.8f1) utilizando Universal Render Pipeline (URP).  
**Autor:** Julian Diaz

---

## Ejecución del Proyecto

El proyecto se encuentra registrado en **Unity Hub** y la escena principal ya está compilada y guardada.

1. Abre **Unity Hub**.
2. Selecciona y abre el proyecto **CubeAdventure**.
3. Presiona el botón **Play** en la barra superior para iniciar el juego directamente.

*(Nota: En caso de requerir reconstruir la escena desde cero, se puede ejecutar el comando de menú `Cube Adventure > Build Complete Level Scene`).*

---

## Controles

| Acción | Entrada Primaria | Entrada Secundaria |
| :--- | :--- | :--- |
| **Movimiento Horizontal** | Teclas `A` / `D` | Flechas `←` / `→` |
| **Salto Variable** | `Barra Espaciadora` | Tecla `W` / Flecha `↑` |
| **Reinicio Rápido** | Tecla `R` | — |

* **Salto corto:** Pulsación breve (corte de gravedad a 2.4x).
* **Salto completo:** Mantener pulsado para alcanzar la altura calibrada de obstáculo.
* **Tolerancia:** Incluye *Coyote Time* (0.15 s) y buffer de entrada (0.15 s).

---

## Mecánicas y Reglas de Juego

* **Objetivo:** Recolectar las **5 monedas de oro** distribuidas a lo largo del recorrido y alcanzar el mástil de meta.
* **Obstáculos y Penalización:** El contacto con cualquier cactus o la caída al vacío reinicia inmediatamente la posición del jugador al punto de partida, restablece el contador de monedas a `00 / 05` y reaparece todas las monedas recolectadas (reinicio limpio en una sola carrera).
* **Condición de Victoria:** Tocar el mástil con las 5 monedas recolectadas activa la fanfarria de victoria, partículas de celebración y la pantalla de fin de nivel con opción de reinicio. Llegar al mástil con menos de 5 monedas desplegará un mensaje indicando la cantidad restante.

---

## Estructura del Repositorio

* **`CubeAdventure/`**: Proyecto base de Unity (versión 6000.6.8f1).
  * **`Assets/_Project/Scripts/`**: Controladores de jugador, cámara, coleccionables, obstáculos, interfaz, audio y editor.
  * **`Assets/_Project/Audio/`**: Efectos de sonido (SFX) y música de fondo ambiental (BGM).
  * **`Assets/_Project/Materials/`**: Materiales URP calibrados para elementos de juego.
  * **`Assets/Scenes/SampleScene.unity`**: Escena principal completamente ensamblada.
* **`Project Design.docx`**: Documento formal de diseño técnico y planificación del proyecto (en inglés).
