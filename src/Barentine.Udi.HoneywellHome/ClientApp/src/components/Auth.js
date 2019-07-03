import React, { Component } from 'react';
// import {LocalStorageComponent} from "../SessionStorageComponent";
const queryString = require('query-string');

export class Auth extends Component {
    static displayName = Auth.name;

    constructor (props) {
        super(props);
        this.state = {clientId: '', clientSecret: '', userId: '', locationsJson: ''};
    }

    componentDidMount() {
        //super.componentDidMount();
        const parsed = queryString.parse(this.props.location.search);
        const clientId = this.getSessionStorageProperty("clientId");
        const clientSecret = this.getSessionStorageProperty("clientSecret");

        if( parsed.code === "" || clientId === "" || clientSecret === "") {
            return;
        }

        fetch('api/auth', {
            method: 'post',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ code: parsed.code, client_id: clientId, client_secret: clientSecret })
        }).then( response => response.json())
            .then( data => {
                this.setState({ userId: data.userId, locationsJson: data.locationsJson });
            })
            .catch( error => {
                console.log(error);
            });
    }

    getSessionStorageProperty(key) {
        if (sessionStorage.hasOwnProperty(key)) {
            let value = sessionStorage.getItem(key);
            try {
                return JSON.parse(value);
            } catch (e) {
            }
        }

        return '';
    }
    
    render () {
        return (
            <div>
                <h1>Counter</h1>

                <p>This is a simple example of a React component.</p>
            </div>
        );
    }
}
