# UX-Bonding
Repo para subir actualizaciones sobre UX Bonding

~~---------------------------------------------------------~~
#  Identificación de requisitos funcionales
- Iniciar la aplicación.
- Mostrar el menú principal.
- Seleccionar jugar o configuración.
- Escribir nombre de jugador.
- Mostrar diálogo, personaje y opciones de respuesta.
- Registrar una respuesta y avanzar por la rama correspondiente.
- Finalizar escena y avanzar al siguiente escenario.
- Guardar progreso y mejor interacción.
- Mostrar ciertos cambios según la acción tomada.
- Volver al menú o abandonar de manera segura.
- Incorporar más escenarios o contextos.
- Configurar tamaño de texto, contraste y volumen.
- Mostrar créditos.
- Agregar audio y visuales.

# Clases
o GameManager
o Player
o Character
o Scene
o Dialogue
o DialogueOption

# Requisitos prioritarios
Requisitos con mayor prioridad:
Los siguientes requisitos son la base del proyecto, lo que permite que el usuario pueda interactuar con el entorno del programa en planeación, por ende son los que primero se deben agregar para poder realizar pruebas tempranas.
- Iniciar nueva partida: Realizar un menú beta es necesario para que el usuario pueda entrar al juego o realizar pruebas, no es necesario hacerlo tan elaborado, solamente con lo esencial para ejecutar el juego.
- Ver diálogos: Ya que básicamente es el gameplay del juego, el escribir diálogos será una parte que tomará bastante tiempo del desarrollo, así que probar desde el inicio pequeños escenarios de interacción es muy importante.
- Seleccionar respuesta: Representa la mecánica en la que se interactúa en el proyecto, ya que permite al usuario tomar decisiones ante situaciones.
- Transición de escenarios: Es necesario para que el juego pueda avanzar y trasladar al jugador a diferentes áreas, todavía se debe planear en que manera se implementarían.


### Requisitos con menor prioridad
Estos requisitos mejoran la experiencia del usuario y manejan la persistencia de datos, pero el ciclo básico del juego puede operar y probarse inicialmente sin ellos.
- Guardar y cargar partida: Permite conservar los datos y el progreso del jugador para que pueda continuar su sesión en otro momento.
- Visualizar retroalimentación: Muestra un resumen o análisis de las consecuencias de las elecciones tomadas al finalizar cada interacción.
- Mostrar créditos y otros detalles visuales: Como se trata de la interfaz, es algo que puede tomar desarrollo en segundo plano, utilizando solamente “placeholders” para el desarrollo del código.
