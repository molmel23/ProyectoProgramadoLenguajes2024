var dataTable;


$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        ajax: {
            "url": "/Admin/MedicoTratante/getall",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "NombreCompleto", "width": "15%" },
            {
                "data": "NumeroColegiado",
                "render": function (data) {
                    return `
                            <a href="/Admin/MedicoTratante/Upsert/${data}" class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>

                            <a onClick=Delete(${data}) class="btn btn-danger mx-2">
                                <i class="bi bi-trash"></i> Delete
                            </a>
                          `
                }
            }
        ]
    });
}

function Delete(_id) {
    Swal.fire({
        title: "Esta seguro?",
        text: "No se puede revertir",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Si, borrar!"
    }).then((result) => {
        if (result.isConfirmed) {

            $.ajax({
                url: "/Admin/MedicoTratante/delete/" + _id,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                },
                error: function () {
                    toastr.error("Error connecting to endpoint");
                }
            });
        }
    });
}