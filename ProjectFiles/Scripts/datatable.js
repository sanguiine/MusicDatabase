$(document).ready(function () {
    $('#example').dataTable({
        "oLanguage": {
            "sSearch": "Szukaj _INPUT_",
            "sEmptyTable": "Tabela nie zawiera ¿adnych danych.",
            "sInfo": "Wyswietla od _START_ do _END_ z _TOTAL_ wszystkich rekordow",
            "sLengthMenu": "Wyswietl _MENU_ rekordow",
            "oPaginate": {
                "sFirst": "Pierwsza strona",
                "sPrevious": "Poprzednia strona",
                "sNext": "Nastepna strona",
                "sLast": "Ostatnia strona"

            }
        }
    });
});