# Guía del Proyecto: Cube Adventure

Prototipo de videojuego 3D en vista lateral (2.5D) desarrollado en Unity 6 con sus documentos de diseño en inglés y español.

---

## 1. Archivos del proyecto

* **Project Design Doc [WORD].docx**: Documento de diseño formal en inglés (plantilla oficial del curso, Autor: Julian Diaz).
* **Project Design Doc [WORD]_ES.docx**: Documento de diseño formal en español (versión complementaria, Autor: Julian Diaz).
* **CubeAdventure/**: Carpeta del proyecto en Unity 6 (versión 6000.6.7f1).
* **PLAN_DE_DESARROLLO.txt**: Registro cronológico de desarrollo y validaciones de compilación.

---

## 2. Cómo abrir y cargar el nivel en Unity

1. Abre **Unity Hub** y selecciona el proyecto **CubeAdventure** (versión Unity 6: 6000.6.7f1).
2. Con Unity abierto, ve a la barra superior de menús y haz clic en:
   **Cube Adventure > Build Complete Level Scene**
   *(Este comando genera y guarda la escena en disco con el cubo rojo, el suelo, las monedas doradas, los cactus y la bandera de meta).*
3. Haz clic en el botón **Play** (icono triangular en la parte superior central) para jugar.

---

## 3. Controles

* **Moverse**: Teclas **A** y **D** (o flechas izquierda y derecha).
* **Saltar**: **Barra espaciadora** (o tecla W / flecha arriba).
  * *Pulsación corta*: Salto bajo y ágil.
  * *Mantener presionado*: Salto a altura completa calibrada para superar el obstáculo.
* **Reinicio rápido**: Tecla **R** (reinicia la partida al instante en cualquier momento).

---

## 4. Objetivo y reglas

1. **Recoger 5 monedas doradas**: Avanza por el nivel recogiendo las monedas giratorias. El contador superior registrará tu avance (`COIN x 00 / 05`).
2. **Esquivar los cactus (Reinicio estilo Geometry Dash)**: Salta los cactus en el camino. La altura del salto está calibrada para superar exactamente el obstáculo al frente y aterrizar antes del siguiente. Si tocas un cactus o caes al vacío, reaparecerás en el punto de inicio, **se te quitan todas las monedas acumuladas (vuelven a 00 / 05) y reaparecen en el escenario**. Debes conseguir las 5 monedas en una sola carrera limpia.
3. **Llegar a la bandera**: Al final del recorrido está el mástil de la bandera. Si llegas con las 5 monedas recolectadas, sonará la fanfarria, se disparará la explosión de partículas verdes de celebración y verás la pantalla de victoria (**STAGE CLEAR!**). Si te faltan monedas, la pantalla te indicará cuántas necesitas para completar el nivel.
