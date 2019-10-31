module MarkdownTable

//type MarkdownTableRow = {
//    Text : string    
//}
(*
|                 August                 |
|----------------------------------------|
|          |  19 |  20 |  21 |  22 |  23 |
|----------|:---:|:---:|:---:|:---:|:---:|
| Imi      |     |  H  |     |     | WFH |
| Jorge    |     |     | WFH |  H  |     |
| Tristian | WFH | WFH | WFH | WFH | WFH |
*)

type TableCell =
    { Text : string }

type TableRow =
    { Header : TableCell
      Cells : TableCell seq }      

let getPadding length (str : string) =
    let diff = length - str.Length
    let paddingLength = diff / 2
    let rightExtra = if diff % 2 = 0 then 0 else 1
    
    (paddingLength, paddingLength + rightExtra)

let fillSpaces = "".PadRight
let fillDashes = fun x -> "".PadRight(x, '-')

let adjustCell length (cell : TableCell) =
    let left, right = getPadding length cell.Text
    sprintf "%s%s%s" (left |> fillSpaces) cell.Text (right |>fillSpaces)

module TableCell =
    let create text = { Text = text }
    
    let empty () = create ""    

module TableRow =
    let toMarkdown headerLength cellLength vSeparator (tr : TableRow) =
        let headerCell = tr.Header |> adjustCell headerLength
        let columns =
            tr.Cells
            |> Seq.fold(fun acc cell -> sprintf "%s%s" acc (cell |> adjustCell cellLength |> sprintf "%s%s" vSeparator)) ""
        
        columns |> sprintf "|%s%s|" headerCell
    
    let create header cells =
        { Header = header; Cells = cells }

type TableHeader = TableCell
type ColumnHeaders = TableCell seq

module TableHeader =
    let toMarkdown length (header : TableHeader) =
        let dashPadding = length |> fillDashes
        
        [ adjustCell length header
          sprintf "|%s|" dashPadding ]

module ColumnHeaders =
    let toMarkdown headerWidth cellWidth vSeparator (columnHeaders : TableCell seq) =
        let separator = 
            //TODO: adjustcell should return with the adjusted TableCell not the text
            let separatorCell = TableCell.create ":-----:"
            
            TableRow.create (TableCell.empty()) (Seq.init (columnHeaders |> Seq.length) (fun _ -> separatorCell))
        
        let headers = TableRow.create (TableCell.empty()) columnHeaders
            
        [ headers; separator ]
        |> Seq.toList
        |> List.map (TableRow.toMarkdown headerWidth cellWidth vSeparator)
     
type Table =
    { Header : TableHeader
      ColumnHeaders : ColumnHeaders
      Rows : TableRow seq
      VerticalSeparator : string }
    
module Table =
     let toMarkdown headerWidth cellWidth (table : Table) =
         let vSeparator = table.VerticalSeparator
         let mainHeaderWidth =
             let columnCount = table.ColumnHeaders |> Seq.length
             (headerWidth + (columnCount * (cellWidth + vSeparator.Length)))

         let mainHeader = table.Header |> TableHeader.toMarkdown mainHeaderWidth
         let headers = table.ColumnHeaders |> ColumnHeaders.toMarkdown headerWidth cellWidth vSeparator
         let rows = table.Rows |> Seq.toList |> List.map (TableRow.toMarkdown headerWidth cellWidth vSeparator) 
        
         mainHeader @ headers @ rows
        
// [<EntryPoint>]
// let main _ =
//     let rowHeaderLength = 15
//     let cellWidth = 7
     
//     let cell = TableCell.create
//     let empty = TableCell.empty
//     let row = TableRow.create
    
//     let mainHeader = 
//         cell "August"
    
//     let columnHeaders = 
//         [cell "19"; cell "20"; cell "21"; cell "22"; cell "23"]
    
//     let rows = 
//         [ row (cell "Imi")
//             [empty(); empty(); empty(); empty(); empty()] 
            
//           row (cell"Tristian")
//             [cell "WFH"; cell"WFH"; cell "H"; cell"ILL"; cell "H"]
                 
//           row (cell "Jorge")
//             [cell "H"; empty(); empty(); cell "1234567"; cell "H"]
                 
//           row (cell "X")
//             [cell "H"; empty(); empty(); cell "WFH"; cell "H"] ]

//     { Header = mainHeader
//       ColumnHeaders = columnHeaders
//       Rows = rows
//       VerticalSeparator = "||" }
//     |> Table.toMarkdown rowHeaderLength cellWidth
//     |> List.iter (printfn "%s")
    
//     0