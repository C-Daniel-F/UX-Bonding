# Arquitectura del Sistema: UX Bonding

Este documento detalla la estructura interna del código de **UX Bonding**, basado en el patrón de diseño **Modelo-Vista-Controlador (MVC)**. Esta arquitectura garantiza que la lógica narrativa, la interfaz de usuario y el estado del juego se mantengan desacoplados y escalables.

---

## Modelos (Datos y Estructura Narrativa)

Los Modelos representan la información del juego. La mayoría de estos datos se gestionan a través de `ScriptableObjects` para optimizar la memoria y facilitar la edición narrativa.

### `Story`
Representa la historia completa del juego y agrupa sus recursos disponibles.
* **Atributos:** `storyId` (string), `storyName` (string), `scenes` (List<StoryScene>), `characters` (List<Character>), `backgrounds` (List<Background>).
* **Métodos:** `GetScene()`, `GetCharacter()`, `GetBackground()`, `ValidateStory()`.

### `StoryScene`
Representa una escena narrativa que contiene una secuencia de nodos de diálogo y música.
* **Atributos:** `sceneId` (string), `sceneName` (string), `audioClip` (AudioClip), `dialogueNodes` (List<DialogueNode>).
* **Métodos:** `GetFirstNode()`, `GetNode()`, `AddDialogueNode()`, `LoadSceneData()`.

### `DialogueNode`
Unidad individual de diálogo que define decisiones y recursos visuales/sonoros del momento.
* **Atributos:** `nodeId` (string), `characterSpeaker` (Character), `characterSpeakerSide` (CharacterSide), `background` (Background), `audioClip` (AudioClip), `text` (string), `next` (StoryDestination), `choices` (List<DialogueChoice>).
* **Métodos:** `GetText()`, `HasChoices()`, `GetChoices()`, `GetNextDestination()`, `IsTerminalNode()`.

### `DialogueChoice` & `StoryDestination`
Definen las opciones del jugador y el flujo de navegación entre escenas.
* **DialogueChoice:** `choiceId` (string), `text` (string), `destination` (StoryDestination). Métodos: `GetText()`, `GetDestination()`, `Select()`, `IsValid()`.
* **StoryDestination:** `sceneId` (string), `nodeId` (string). Métodos: `IsValid()`, `GetSceneId()`, `GetNodeId()`, `IsSameDestination()`.

### Entidades Visuales (`Character` & `Background`)
* **Character:** `characterId` (string), `displayName` (string), `sprite` (Sprite).
* **Background:** `backgroundId` (string), `displayName` (string), `image` (Sprite).

### `GameState`
Almacena el estado en tiempo de ejecución (partida guardada/activa).
* **Atributos:** `flags` (Dictionary<string, bool>), `relationships` (Dictionary<string, int>).
* **Métodos:** `SetFlag()`, `GetFlag()`, `SetRelationship()`, `GetRelationship()`, `Reset()`.

---

## Controladores (Lógica y Flujo)

Los Controladores intermedian entre los datos narrativos y lo que el usuario ve en pantalla.

### `StoryBootstrap`
Punto de entrada. Inicializa los componentes principales y conecta el MVC.
* **Atributos:** `story`, `storyRuntime`, presentadores de UI, audio y personajes.
* **Métodos:** `Initialize()`, `StartStory()`, `BindPresenters()`, `LoadInitialState()`, `RestartStory()`.

### `StoryRuntime`
Motor principal. Administra el nodo actual y procesa el avance lógico del jugador.
* **Atributos:** `story` (Story), `currentScene` (StoryScene), `currentNode` (DialogueNode), `gameState` (GameState).
* **Métodos:** `Start()`, `Advance()`, `Choose(DialogueChoice)`, `LoadDestination()`, `Stop()`, `IsFinished()`.

### Presentadores Específicos
Conectan los datos (Modelos) con la interfaz de Unity (Vistas).
* **`DialoguePresenter`:** Solicita avances de texto y procesa elecciones enviándolas al `StoryRuntime`.
* **`CharacterPresenter`:** Lógica para mostrar/ocultar y cambiar el énfasis (iluminación/tamaño) de los personajes.
* **`BackgroundPresenter`:** Lógica de transiciones y fundidos (fades) de escenarios.
* **`AudioPresenter`:** Gestión de canales separados para `musicSource` (BG) y `effectSource` (SFX).

---

## Vistas (Interfaz de Usuario - Unity UI)

Componentes `MonoBehaviour` estrictamente visuales. No contienen lógica de juego, solo reaccionan a las órdenes de los Presentadores.

* **`DialogueUI`:** Maneja los componentes `Text` (o TextMeshPro) para el diálogo, el nombre del hablante y el contenedor `Transform` que instancia los botones de respuesta (`choiceButtonPrefab`).
* **`CharacterSide` (Enum):** Define anclajes en pantalla (`Left`, `Right`, `Center`, `None`).
