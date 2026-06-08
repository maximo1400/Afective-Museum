<?php
error_reporting(E_ALL);
ini_set('display_errors', 1);
// Ruta al archivo CSV
$filename = "guestbook.csv";

// Asegura que el archivo existe y tiene encabezado
if (!file_exists($filename)) {
    file_put_contents($filename, "Name,Message\n");
}

// Solo aceptar POST
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $name = isset($_POST['name']) ? trim($_POST['name']) : "";
    $message = isset($_POST['message']) ? trim($_POST['message']) : "";

    // Sanitiza y quita comas
    $name = str_replace(",", "", $name);
    $message = str_replace(",", "", $message);

    // Agrega al archivo CSV
    $line = "$name,$message\n";
    file_put_contents($filename, $line, FILE_APPEND | LOCK_EX);

    echo "Message saved correctly";
} else {
    echo "Forbidden method";
}
?>
