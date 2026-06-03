# TCP Socket Client for Afective Museum

This folder contains the `TcpSocketClient.cs` script which connects to a TCP server on the loopback address (`127.0.0.1`).

## How to run this script on Museum start

To ensure this script runs as soon as the Afective Museum application starts, follow these steps in the Unity Editor:

1. **Open the Initial Scene:** Open the very first scene that is loaded when the museum starts (this might be named something like `MainMenu`, `StartScene`, `MuseumMain`, etc.).
2. **Create a Manager Object:**
   - Right-click in the Hierarchy window and select **Create Empty**.
   - Rename this new GameObject to something descriptive, like `NetworkManager` or `TcpClientManager`.
3. **Attach the Script:**
   - Select your newly created `NetworkManager` GameObject in the Hierarchy.
   - In the Inspector window, click **Add Component**.
   - Search for `Tcp Socket Client` and select it. Alternatively, you can simply drag and drop the `TcpSocketClient.cs` script from the `Assets/TcpClient` folder onto the GameObject in the Inspector.
4. **Configure Settings:**
   - The script defaults to connecting to `127.0.0.1` on port `8080`.
   - You can change these values directly in the Unity Inspector under the "TCP Connection Settings" header on the script component.
5. **Persist Across Scenes (Optional):**
   - If you need the TCP connection to stay active even if the scene changes (e.g., moving between different museum rooms), you should add another simple script to the `NetworkManager` GameObject that calls `DontDestroyOnLoad(gameObject);` in its `Awake()` method.

When you hit Play (or run the built application), the `Start()` method in `TcpSocketClient.cs` will automatically be called, initiating the connection to the TCP socket on loopback.
