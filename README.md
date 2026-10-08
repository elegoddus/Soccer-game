# Soccer Kick Test

Unity recruitment test implementing a small top-down soccer interaction using the supplied assets.

## Controls

- `W A S D`: move Jammo.
- `KICK`: appears only while Jammo is close to an available ball.
- `AUTO KICK`: shoots the nearest available ball toward the positive-X goal.
- `RESET`: reloads the gameplay scene.

The camera follows Jammo, switches to the ball during a shot, holds on the goal for two seconds after scoring, then returns to Jammo. A `Confetti Explosion - Stars` effect plays when the ball enters the goal.

## Project structure

- `Assets/SoccerTest/Scripts`: runtime gameplay components.
- `Assets/SoccerTest/Editor/SoccerSceneBuilder.cs`: reproducible scene and Windows build setup.
- `Assets/Scenes/Location soccer field.unity`: gameplay scene.

Use **Soccer Test > Build Gameplay Scene** to regenerate the scene, or run `SoccerSceneBuilder.BuildWindowsPlayer` in batch mode to create `Build/Windows/SoccerTest.exe`.
