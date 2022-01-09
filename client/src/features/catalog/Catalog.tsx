import {Product} from "../../app/models/product";
import ProductList from "./ProductList";
import {useEffect, useState} from "react";


export default function Catalog() {
    const [products, setProduct] = useState<Product[]>([]);

    useEffect(() => {
        fetch('http://localhost:5000/api/products')
            .then(res => res.json())
            .then(data => setProduct(data))
    }, [])

    return (
        <>
            <ProductList products={products}/>
        </>
    )
}
