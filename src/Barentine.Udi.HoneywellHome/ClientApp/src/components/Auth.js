import React, { Component } from 'react';
import SyntaxHighlighter from 'react-syntax-highlighter';
import { docco } from 'react-syntax-highlighter/dist/esm/styles/hljs';
import {
    DataTable,
    TableHeader,
    TableBody,
    TableRow,
    TableColumn,
} from 'react-md';

const queryString = require('query-string');

export class Auth extends Component {
    static displayName = Auth.name;

    constructor (props) {
        super(props);
        this.state = {clientId: '', clientSecret: '', locations: '', users: '', error: ''};
    }

    componentDidMount() {
        const parsed = queryString.parse(this.props.location.search);
        const clientId = this.getSessionStorageProperty("clientId");
        const clientSecret = this.getSessionStorageProperty("clientSecret");

        if( parsed.code === "" || clientId === "" || clientSecret === "") {
            return;
        }

        fetch('api/metadata/users', {
            method: 'post',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ code: parsed.code, client_id: clientId, client_secret: clientSecret })
        }).then( response => response.json())
            .then( data => {
                this.setState({ locations: data.locations, users: data.users, error: data.errorMessage });
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
                <h1>Response</h1>
                <ErrorMessage error={this.state.error} />
                <UserTable users={this.state.users} />
                <br />
                <SyntaxHighlighter language="json" style={docco}>
                    {JSON.stringify(this.state.locations, undefined, 2)}
                </SyntaxHighlighter>
            </div>
        );
    }
}

const ErrorMessage = ({ error }) => {
    if(!error) {
        return (
            <div />
        );
    }
    
    return (
        <p style={{color: 'red'}}>{error}</p>
    );
};

const UserTable = ({ users }) => {
    if(!users) {
        return (
            <div id="users" />
        );
    }

    return (
        <DataTable plain>
            <TableHeader>
                <TableRow>
                    <TableColumn>User ID</TableColumn>
                    <TableColumn>Username</TableColumn>
                    <TableColumn>First Name</TableColumn>
                    <TableColumn>Last Name</TableColumn>
                </TableRow>
            </TableHeader>
            <TableBody>
                {users.map((_, i) => (
                    <TableRow key={_.userId}>
                        <TableColumn>{_.userId}</TableColumn>
                        <TableColumn>{_.username}</TableColumn>
                        <TableColumn>{_.firstName}</TableColumn>
                        <TableColumn>{_.lastName}</TableColumn>
                    </TableRow>
                ))}
            </TableBody>
        </DataTable>
    );
};
