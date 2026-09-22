# UX-Bonding

Repo para subir actualizaciones sobre UX Bonding

~~---------------------------------------------------------~~

# Identificación de requisitos funcionales

* Iniciar una partida/historia.
* Mostrar diálogo, personaje y opciones de respuesta.
* Registrar una respuesta y avanzar por la rama correspondiente.
* Finalizar una escena narrativa y avanzar al siguiente escenario.
* Mostrar cambios de contexto según la acción tomada.
* Mostrar fondos y personajes en pantalla.
* Reproducir música asociada al escenario.
* Reproducir efectos de sonido asociados a nodos sin interrumpir la música.
* Mantener música entre escenarios mientras no exista una nueva que la reemplace.
* Incorporar más escenarios, ramas y contextos narrativos.
* Manejar el estado de la partida mediante banderas y relaciones entre personajes.

# Clases

o Story

o StoryScene

o DialogueNode

o DialogueChoice

o StoryDestination
                
o Character

o Background

o GameState

o StoryRuntime

o CharacterSide

o DialogueUI

o DialoguePresenter

o CharacterPresenter

o BackgroundPresenter

o AudioPresenter

o StoryBootstrap

# Requisitos prioritarios

Requisitos con mayor prioridad:
Los siguientes requisitos son la base del proyecto y permiten probar el ciclo principal del juego de forma temprana.

* Iniciar nueva partida: Permitir iniciar una historia desde el sistema de prueba y comenzar desde su primer nodo.
* Ver diálogos: Mostrar el personaje, nombre del hablante y texto correspondiente al nodo actual.
* Seleccionar respuesta: Permitir seleccionar opciones y avanzar por la rama correspondiente.
* Transición de escenarios narrativos: Permitir que los nodos y elecciones dirijan hacia diferentes escenas narrativas.
* Mostrar personajes: Mostrar uno o más personajes en pantalla y mantenerlos presentes mientras la conversación continúa.
* Mostrar fondos: Mostrar y cambiar el fondo según el escenario narrativo.
* Reproducir audio: Reproducir música por escenario y efectos de sonido por nodo sin interrumpir la música actual.

### Requisitos con menor prioridad

Estos requisitos amplían el sistema y permiten manejar historias más complejas, pero no son necesarios para probar inicialmente el ciclo básico del juego.

* Manejar estado narrativo: Registrar banderas y relaciones entre personajes para permitir futuras condiciones y consecuencias.
* Ampliar escenarios y ramas: Incorporar nuevas escenas, personajes, fondos y decisiones sin modificar el código del sistema.
* Mejorar presentación visual y sonora: Agregar posteriormente animaciones, expresiones, transiciones, cambios de enfoque, mezcla de audio y otros elementos de presentación.
