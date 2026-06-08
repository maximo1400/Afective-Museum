<?php
$filename = "guestbook.csv";

if (file_exists($filename)) {
    // Devuelve el contenido plano del CSV
    header('Content-Type: text/plain');
    readfile($filename);
} else {
    echo "No Messages yet";
}
?>
