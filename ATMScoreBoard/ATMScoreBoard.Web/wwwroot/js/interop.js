// Este archivo contendrá funciones que C# puede invocar.

window.Swal_confirm = (title, html, icon, confirmButtonText) => {
    return Swal.fire({
        title: title,
        html: html,
        icon: icon,
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: confirmButtonText,
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        // Devolvemos 'true' solo si el usuario hizo clic en el botón de confirmación
        return result.isConfirmed;
    });
}